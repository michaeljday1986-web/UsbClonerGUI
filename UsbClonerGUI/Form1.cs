using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Text;
using System.Drawing;   // <- add this line

namespace UsbClonerGUI
{
    public partial class Form1 : Form
    {
        // currently unused, but kept in case we later re-enable letter preservation
        private List<string> _savedTargetLetters = new List<string>();

        // Class to hold USB disk info
        private class DiskInfo
        {
            public int Number { get; set; }
            public string Model { get; set; } = "";
            public long Size { get; set; }

            public override string ToString()
            {
                double gb = Size / (1024.0 * 1024 * 1024);
                return $"Disk {Number} - {gb:0.0} GB - {Model}";
            }
        }

        // Partition info for adjusting capacity
        private class PartitionInfo
        {
            public uint Index { get; set; }
            public ulong StartingOffset { get; set; }
            public ulong Size { get; set; }
            public string Type { get; set; } = "";
            public string FileSystem { get; set; } = "";
        }

        public Form1()
        {
            InitializeComponent();
            this.Load += Form1_Load;
            btnRefresh.Click += BtnRefresh_Click;
            btnClone.Click += BtnClone_Click;

            progressBarClone.Value = 0;
            labelStatus.Text = "Status: Idle";
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadUsbDisks();

            // Start disabled until everything is valid
            btnClone.Enabled = false;

            // Re-check whenever any of these change
            chkConfirm.CheckedChanged += (s, ev) => UpdateCloneButtonEnabled();
            comboSource.SelectedIndexChanged += (s, ev) => UpdateCloneButtonEnabled();
            comboTarget.SelectedIndexChanged += (s, ev) => UpdateCloneButtonEnabled();

            // THEME: start in light mode
            ApplyTheme(false);

            // Toggle when Dark Mode checkbox changes
            chkDarkMode.CheckedChanged += (s, ev) =>
            {
                ApplyTheme(chkDarkMode.Checked);
            };
        }



        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadUsbDisks();
        }

        private async void BtnClone_Click(object sender, EventArgs e)
        {
            if (!chkConfirm.Checked)
            {
                MessageBox.Show(
                    "Please tick the confirmation checkbox to acknowledge that the target disk will be erased.",
                    "Confirmation required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var src = comboSource.SelectedItem as DiskInfo;
            var dst = comboTarget.SelectedItem as DiskInfo;

            if (src == null || dst == null)
            {
                MessageBox.Show("Please select both a source and a target USB disk.", "Missing selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (src.Number == dst.Number)
            {
                MessageBox.Show("Source and target cannot be the same disk.", "Invalid selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (dst.Size < src.Size)
            {
                MessageBox.Show("Target disk is smaller than source disk. Aborting.", "Size error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var result = MessageBox.Show(
                $"You are about to CLONE:\n\n" +
                $"FROM: {src}\n" +
                $"TO:   {dst}\n\n" +
                $"This will ERASE ALL DATA on the target disk.\n\nContinue?",
                "Confirm clone",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            // Disable UI while cloning
            SetUiEnabled(false);
            progressBarClone.Value = 0;
            labelStatus.Text = "Status: Cloning...";
            txtLog.Clear();
            AppendLog("=== USB Build Stick Cloner ===");
            AppendLog($"Source: {src}");
            AppendLog($"Target: {dst}");
            AppendLog("Starting FILE-LEVEL clone (no raw sectors)...");

            try
            {
                var progress = new Progress<int>(percent =>
                {
                    if (percent < 0) percent = 0;
                    if (percent > 100) percent = 100;
                    progressBarClone.Value = percent;
                    labelStatus.Text = $"Status: Cloning... {percent}%";
                });

                await Task.Run(() => CloneUsbByFileCopy(src.Number, dst.Number, progress));

                AppendLog("File-level clone completed.");
                labelStatus.Text = "Status: Complete";
            }
            catch (Exception ex)
            {
                AppendLog("ERROR: " + ex.Message);
                MessageBox.Show("An error occurred:\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                labelStatus.Text = "Status: Error";
            }
            finally
            {
                SetUiEnabled(true);
            }
        }


        private void SetUiEnabled(bool enabled)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<bool>(SetUiEnabled), enabled);
                return;
            }

            comboSource.Enabled = enabled;
            comboTarget.Enabled = enabled;
            btnRefresh.Enabled = enabled;
            btnClone.Enabled = enabled;
            chkConfirm.Enabled = enabled;
        }

        private void AppendLog(string message)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<string>(AppendLog), message);
                return;
            }

            txtLog.AppendText(message + Environment.NewLine);
        }

        // ========== USB DISK ENUMERATION ==========

        private void LoadUsbDisks()
        {
            try
            {
                var disks = GetUsbDisks();
                comboSource.Items.Clear();
                comboTarget.Items.Clear();

                foreach (var d in disks)
                {
                    comboSource.Items.Add(d);
                    comboTarget.Items.Add(d);
                }

                if (disks.Count > 0)
                {
                    comboSource.SelectedIndex = 0;
                    if (disks.Count > 1)
                        comboTarget.SelectedIndex = 1;
                }
                UpdateCloneButtonEnabled();


                AppendLog($"Found {disks.Count} USB disk(s).");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to load USB disks:\n\n" + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private List<DiskInfo> GetUsbDisks()
        {
            var results = new List<DiskInfo>();

            using (var searcher = new ManagementObjectSearcher(
                       "SELECT DeviceID,Index,Model,Size,InterfaceType,MediaType FROM Win32_DiskDrive"))
            {
                foreach (ManagementObject drive in searcher.Get())
                {
                    try
                    {
                        string interfaceType = (drive["InterfaceType"] as string) ?? "";
                        string mediaType = (drive["MediaType"] as string) ?? "";
                        bool isUsb = interfaceType.Equals("USB", StringComparison.OrdinalIgnoreCase)
                                     || mediaType.IndexOf("Removable", StringComparison.OrdinalIgnoreCase) >= 0;

                        if (!isUsb) continue;

                        int index = Convert.ToInt32(drive["Index"]);
                        long size = Convert.ToInt64(drive["Size"]);
                        string model = (drive["Model"] as string) ?? "Unknown";

                        results.Add(new DiskInfo
                        {
                            Number = index,
                            Model = model,
                            Size = size
                        });
                    }
                    catch
                    {
                        // Ignore bad entries
                    }
                }
            }

            return results.OrderBy(d => d.Number).ToList();
        }

        // ========== CLONE LOGIC ==========

        private void CloneDisk(int srcDiskNumber, int dstDiskNumber, long bytesToCopy, IProgress<int> progress)
        {
            string srcPath = $@"\\.\PhysicalDrive{srcDiskNumber}";
            string dstPath = $@"\\.\PhysicalDrive{dstDiskNumber}";

            AppendLog($"Cloning from {srcPath} to {dstPath}");
            const int bufferSize = 1024 * 1024 * 16; // 16 MB

            var buffer = new byte[bufferSize];
            long totalCopied = 0;
            var sw = Stopwatch.StartNew();

            FileStream src = null;
            FileStream dst = null;

            try
            {
                // --- Open source disk ---
                try
                {
                    AppendLog("Opening source disk for read...");
                    src = new FileStream(
                        srcPath,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite | FileShare.Delete);

                    AppendLog("Source disk opened successfully.");
                }
                catch (Exception ex)
                {
                    AppendLog("FAILED to open source disk: " + ex.Message);
                    throw new Exception("Failed to open source disk " + srcPath + ": " + ex.Message, ex);
                }

                // --- Open target disk ---
                try
                {
                    AppendLog("Opening target disk for read/write...");
                    dst = new FileStream(
                        dstPath,
                        FileMode.Open,
                        FileAccess.ReadWrite,
                        FileShare.None);   // exclusive access


                    AppendLog("Target disk opened successfully.");
                }
                catch (Exception ex)
                {
                    AppendLog("FAILED to open target disk: " + ex.Message);
                    throw new Exception("Failed to open target disk " + dstPath + ": " + ex.Message, ex);
                }

                long remaining = bytesToCopy;
                int lastPercent = 0;

                src.Position = 0;
                dst.Position = 0;

                AppendLog("Starting sector copy loop...");

                while (remaining > 0)
                {
                    int toRead = (int)Math.Min(bufferSize, remaining);
                    int read = 0;

                    // --- Read from source ---
                    try
                    {
                        read = src.Read(buffer, 0, toRead);
                    }
                    catch (Exception ex)
                    {
                        AppendLog("READ FAILED from source disk: " + ex.Message);
                        throw;
                    }

                    if (read <= 0)
                    {
                        AppendLog("Reached unexpected end of source disk.");
                        break;
                    }

                    // --- Write to target ---
                    try
                    {
                        dst.Write(buffer, 0, read);
                    }
                    catch (Exception ex)
                    {
                        AppendLog("WRITE FAILED to target disk: " + ex.Message);
                        throw;
                    }

                    totalCopied += read;
                    remaining -= read;

                    int percent = (int)(totalCopied * 100 / bytesToCopy);
                    if (percent != lastPercent)
                    {
                        lastPercent = percent;
                        progress.Report(percent);

                        double mbCopied = totalCopied / (1024.0 * 1024);
                        double seconds = sw.Elapsed.TotalSeconds;
                        double mbPerSec = seconds > 0 ? (mbCopied / seconds) : 0;

                        AppendLog($"Progress: {percent}%  ({mbCopied:0} MB, {mbPerSec:0.0} MB/s)");
                    }
                }

                try
                {
                    dst.Flush(true);
                }
                catch (Exception ex)
                {
                    AppendLog("Flush to target disk FAILED: " + ex.Message);
                    throw;
                }

                AppendLog("Clone operation finished.");
            }
            finally
            {
                try { src?.Dispose(); } catch { }
                try { dst?.Dispose(); } catch { }
                sw.Stop();
            }
        }

        private void UpdateCloneButtonEnabled()
        {
            var src = comboSource.SelectedItem as DiskInfo;
            var dst = comboTarget.SelectedItem as DiskInfo;

            bool validSelection = src != null && dst != null && src.Number != dst.Number;
            bool ok = validSelection && chkConfirm.Checked;

            btnClone.Enabled = ok;
        }

        private void ApplyTheme(bool dark)
        {
            if (dark)
            {
                // Base colors
                var formBack = Color.FromArgb(32, 32, 32);
                var groupBack = Color.FromArgb(45, 45, 48);
                var textColor = Color.WhiteSmoke;
                var accentBack = Color.FromArgb(63, 63, 70);

                // Form
                this.BackColor = formBack;
                this.ForeColor = textColor;

                // Title / subtitle
                lblTitle.ForeColor = textColor;
                label3.ForeColor = Color.Gainsboro;

                // Group boxes
                groupBoxDisks.BackColor = groupBack;
                groupBoxDisks.ForeColor = textColor;

                groupBoxActions.BackColor = groupBack;
                groupBoxActions.ForeColor = textColor;

                // Labels
                label1.ForeColor = textColor;
                label2.ForeColor = textColor;
                labelStatus.ForeColor = textColor;

                // Checkboxes
                chkConfirm.ForeColor = textColor;
                chkDarkMode.ForeColor = textColor;

                // Buttons – custom style in dark mode
                btnRefresh.UseVisualStyleBackColor = false;
                btnClone.UseVisualStyleBackColor = false;
                btnRefresh.FlatStyle = FlatStyle.Flat;
                btnClone.FlatStyle = FlatStyle.Flat;
                btnRefresh.BackColor = accentBack;
                btnClone.BackColor = accentBack;
                btnRefresh.ForeColor = textColor;
                btnClone.ForeColor = textColor;


                // Combos
                comboSource.BackColor = Color.FromArgb(37, 37, 38);
                comboSource.ForeColor = textColor;
                comboTarget.BackColor = Color.FromArgb(37, 37, 38);
                comboTarget.ForeColor = textColor;

                // Progress bar background
                progressBarClone.BackColor = groupBack;

                // Log window
                txtLog.BackColor = Color.FromArgb(30, 30, 30);
                txtLog.ForeColor = Color.Gainsboro;
            }
            else
            {
                // Light mode (defaults)
                this.BackColor = SystemColors.Control;
                this.ForeColor = SystemColors.ControlText;

                lblTitle.ForeColor = SystemColors.ControlText;
                label3.ForeColor = Color.Gray;

                groupBoxDisks.BackColor = SystemColors.Control;
                groupBoxDisks.ForeColor = SystemColors.ControlText;

                groupBoxActions.BackColor = SystemColors.Control;
                groupBoxActions.ForeColor = SystemColors.ControlText;

                label1.ForeColor = SystemColors.ControlText;
                label2.ForeColor = SystemColors.ControlText;
                labelStatus.ForeColor = SystemColors.ControlText;

                chkConfirm.ForeColor = SystemColors.ControlText;
                chkDarkMode.ForeColor = SystemColors.ControlText;

                // Buttons – use system theme in light mode
                btnRefresh.UseVisualStyleBackColor = true;
                btnClone.UseVisualStyleBackColor = true;
                btnRefresh.FlatStyle = FlatStyle.Standard;
                btnClone.FlatStyle = FlatStyle.Standard;
                btnRefresh.BackColor = SystemColors.Control;
                btnClone.BackColor = SystemColors.Control;
                btnRefresh.ForeColor = SystemColors.ControlText;
                btnClone.ForeColor = SystemColors.ControlText;


                comboSource.BackColor = SystemColors.Window;
                comboSource.ForeColor = SystemColors.WindowText;
                comboTarget.BackColor = SystemColors.Window;
                comboTarget.ForeColor = SystemColors.WindowText;

                progressBarClone.BackColor = SystemColors.Control;

                txtLog.BackColor = SystemColors.Window;
                txtLog.ForeColor = SystemColors.WindowText;
            }
        }

        private void chkDarkMode_CheckedChanged(object sender, EventArgs e)
        {
            ApplyDarkMode(chkDarkMode.Checked);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            bool isDark = (this.BackColor.R < 50);  // quick dark check

            MessageBox.Show(
                "USB Build Stick Cloner\n" +
                "Version 1.0.0\n\n" +
                "Clones USB BOOT+DATA sticks using partition-preserving file copy.\n\n" +
                "© 2025 Michael Day",
                "About USB Build Stick Cloner",
                MessageBoxButtons.OK,
                isDark ? MessageBoxIcon.None : MessageBoxIcon.Information
            );
        }



        private void ApplyDarkMode(bool dark)
        {
            Color back = dark ? Color.FromArgb(45, 45, 48) : SystemColors.Control;
            Color fore = dark ? Color.White : Color.Black;
            Color box = dark ? Color.FromArgb(28, 28, 28) : SystemColors.Window;

            this.BackColor = back;

            foreach (Control c in this.Controls)
                ApplyControlTheme(c, back, fore, box);
        }

        private void ApplyControlTheme(Control c, Color back, Color fore, Color box)
        {
            if (c is TextBox)
                c.BackColor = box;

            c.BackColor = back;
            c.ForeColor = fore;

            foreach (Control child in c.Controls)
                ApplyControlTheme(child, back, fore, box);
        }


        private void CloneUsbByFileCopy(int srcDiskNumber, int dstDiskNumber, IProgress<int> progress)
        {
            AppendLog("Detecting partitions on SOURCE disk...");

            var srcParts = GetPartitions(srcDiskNumber);
            if (srcParts.Count == 0)
                throw new Exception("Source disk has no partitions.");

            // --- CASE 1: two-part layout: FAT32 boot + NTFS data ---
            var fat32Parts = srcParts.Where(p => string.Equals(p.FileSystem, "FAT32", StringComparison.OrdinalIgnoreCase)).ToList();
            var ntfsParts = srcParts.Where(p => string.Equals(p.FileSystem, "NTFS", StringComparison.OrdinalIgnoreCase)).ToList();

            if (fat32Parts.Count >= 1 && ntfsParts.Count >= 1)
            {
                // pick first FAT32 by offset as boot, first NTFS as data
                var srcBoot = fat32Parts.OrderBy(p => p.StartingOffset).First();
                var srcData = ntfsParts.OrderBy(p => p.StartingOffset).First();

                string srcBootRoot = GetDriveLetterForPartition(srcDiskNumber, srcBoot.Index);
                string srcDataRoot = GetDriveLetterForPartition(srcDiskNumber, srcData.Index);

                if (string.IsNullOrEmpty(srcBootRoot) || string.IsNullOrEmpty(srcDataRoot))
                    throw new Exception("Could not determine drive letters for source partitions.");

                AppendLog($"Source BOOT partition: {srcBootRoot} (FAT32)");
                AppendLog($"Source DATA partition: {srcDataRoot} (NTFS)");

                // Create BOOT + DATA on target
                AppendLog("Creating BOOT + DATA partitions on target disk...");

                var sb = new StringBuilder();
                sb.AppendLine($"select disk {dstDiskNumber}");
                sb.AppendLine("online disk noerr");
                sb.AppendLine("attributes disk clear readonly noerr");
                sb.AppendLine("clean");
                sb.AppendLine("convert mbr");
                sb.AppendLine("create partition primary size=2048");
                sb.AppendLine("format fs=fat32 quick label=BOOT");
                sb.AppendLine("assign");
                sb.AppendLine("create partition primary");
                sb.AppendLine("format fs=ntfs quick label=DATA");
                sb.AppendLine("assign");
                RunDiskpartScript(sb.ToString());

                System.Threading.Thread.Sleep(1000);

                var dstParts = GetPartitions(dstDiskNumber);
                if (dstParts.Count < 2)
                    throw new Exception("Failed to create BOOT + DATA partitions on target disk. See diskpart output in the log.");

                var dstBoot = dstParts.OrderBy(p => p.StartingOffset).First();
                var dstData = dstParts.OrderBy(p => p.StartingOffset).Skip(1).First();

                string dstBootRoot = GetDriveLetterForPartition(dstDiskNumber, dstBoot.Index);
                string dstDataRoot = GetDriveLetterForPartition(dstDiskNumber, dstData.Index);

                if (string.IsNullOrEmpty(dstBootRoot) || string.IsNullOrEmpty(dstDataRoot))
                    throw new Exception("Could not determine drive letters for target partitions after creation.");

                AppendLog($"Target BOOT partition: {dstBootRoot}");
                AppendLog($"Target DATA partition: {dstDataRoot}");

                // Copy BOOT (0–30%) and DATA (30–100%)
                AppendLog("Copying BOOT (FAT32) files...");
                RunRobocopy(srcBootRoot, dstBootRoot, progress, 0, 30);

                AppendLog("Copying DATA (NTFS) files...");
                RunRobocopy(srcDataRoot, dstDataRoot, progress, 30, 100);

                AppendLog("File-level clone (BOOT + DATA) completed.");
                return;
            }

            // --- CASE 2: single-part layout (e.g. one FAT32 or NTFS partition) ---
            if (srcParts.Count == 1)
            {
                var srcPart = srcParts[0];
                string fs = srcPart.FileSystem?.ToUpperInvariant() ?? "";

                string srcRoot = GetDriveLetterForPartition(srcDiskNumber, srcPart.Index);
                if (string.IsNullOrEmpty(srcRoot))
                    throw new Exception("Could not determine drive letter for source partition.");

                AppendLog($"Source has a single {fs} partition: {srcRoot}");

                // Read target disk size (for logging / future use)
                long diskSize;
                try
                {
                    var driveObj = new ManagementObjectSearcher(
                        $"SELECT Size FROM Win32_DiskDrive WHERE Index = {dstDiskNumber}")
                        .Get().Cast<ManagementObject>().FirstOrDefault();
                    diskSize = driveObj != null ? Convert.ToInt64(driveObj["Size"]) : 0;
                }
                catch
                {
                    diskSize = 0;
                }

                // Size of source partition in MB
                ulong srcSizeMbU = srcPart.Size / (1024UL * 1024UL);
                if (srcSizeMbU == 0) srcSizeMbU = 1024; // fallback 1GB
                if (srcSizeMbU > int.MaxValue) srcSizeMbU = int.MaxValue;
                int srcSizeMb = (int)srcSizeMbU;

                // If single FAT32: create a FAT32 partition approx same size as source, plus NTFS for the rest
                if (fs == "FAT32")
                {
                    AppendLog($"Creating FAT32 BOOT partition (~{srcSizeMb} MB) plus NTFS DATA partition on target disk...");

                    var sb = new StringBuilder();
                    sb.AppendLine($"select disk {dstDiskNumber}");
                    sb.AppendLine("online disk noerr");
                    sb.AppendLine("attributes disk clear readonly noerr");
                    sb.AppendLine("clean");
                    sb.AppendLine("convert mbr");
                    sb.AppendLine($"create partition primary size={srcSizeMb}");
                    sb.AppendLine("format fs=fat32 quick label=BOOT");
                    sb.AppendLine("assign");
                    sb.AppendLine("create partition primary");
                    sb.AppendLine("format fs=ntfs quick label=DATA");
                    sb.AppendLine("assign");
                    RunDiskpartScript(sb.ToString());

                    System.Threading.Thread.Sleep(1000);

                    var dstParts = GetPartitions(dstDiskNumber);
                    if (dstParts.Count < 1)
                        throw new Exception("Failed to create partitions on target disk. See diskpart output in the log.");

                    var dstBoot = dstParts.OrderBy(p => p.StartingOffset).First();
                    string dstBootRoot = GetDriveLetterForPartition(dstDiskNumber, dstBoot.Index);

                    if (string.IsNullOrEmpty(dstBootRoot))
                        throw new Exception("Could not determine drive letter for target BOOT partition after creation.");

                    AppendLog($"Target BOOT partition: {dstBootRoot} (FAT32)");
                    AppendLog("Copying all files from source to BOOT partition (single-part FAT32 layout)...");
                    RunRobocopy(srcRoot, dstBootRoot, progress, 0, 100);

                    AppendLog("File-level clone (single-part FAT32 layout) completed.");
                    return;
                }
                else
                {
                    // Single NTFS or unknown -> create one full-size NTFS partition and copy everything
                    string targetFs = "NTFS";

                    AppendLog($"Creating single {targetFs} partition on target disk (full capacity)...");
                    var sb = new StringBuilder();
                    sb.AppendLine($"select disk {dstDiskNumber}");
                    sb.AppendLine("online disk noerr");
                    sb.AppendLine("attributes disk clear readonly noerr");
                    sb.AppendLine("clean");
                    sb.AppendLine("convert mbr");
                    sb.AppendLine("create partition primary");
                    sb.AppendLine($"format fs={targetFs.ToLowerInvariant()} quick label=USB");
                    sb.AppendLine("assign");
                    RunDiskpartScript(sb.ToString());

                    System.Threading.Thread.Sleep(1000);

                    var dstParts = GetPartitions(dstDiskNumber);
                    if (dstParts.Count < 1)
                        throw new Exception("Failed to create partition on target disk. See diskpart output in the log.");

                    var dstPart = dstParts.OrderBy(p => p.StartingOffset).First();
                    string dstRoot = GetDriveLetterForPartition(dstDiskNumber, dstPart.Index);

                    if (string.IsNullOrEmpty(dstRoot))
                        throw new Exception("Could not determine drive letter for target partition after creation.");

                    AppendLog($"Target partition: {dstRoot} ({targetFs})");
                    AppendLog("Copying all files (single-part NTFS/other layout)...");
                    RunRobocopy(srcRoot, dstRoot, progress, 0, 100);

                    AppendLog("File-level clone (single-part NTFS layout) completed.");
                    return;
                }
            }

            // --- CASE 3: anything else (weird layouts) ---
            throw new Exception("Source disk layout is not supported (expected either FAT32+NTFS or a single partition).");
        }



        private List<PartitionInfo> GetPartitions(int diskNumber)
        {
            var result = new List<PartitionInfo>();

            using (var searcher = new ManagementObjectSearcher(
                       $"SELECT * FROM Win32_DiskPartition WHERE DiskIndex = {diskNumber}"))
            {
                foreach (ManagementObject part in searcher.Get())
                {
                    try
                    {
                        uint index = (uint)part["Index"];
                        ulong offset = (ulong)part["StartingOffset"];
                        ulong size = (ulong)part["Size"];
                        string type = (part["Type"] as string) ?? "";

                        string fs = GetFileSystemForPartition(part);

                        result.Add(new PartitionInfo
                        {
                            Index = index,
                            StartingOffset = offset,
                            Size = size,
                            Type = type,
                            FileSystem = fs
                        });
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            return result;
        }

        private string GetFileSystemForPartition(ManagementObject partition)
        {
            try
            {
                var query = new ManagementObjectSearcher(
                    $"ASSOCIATORS OF {{{partition.Path.RelativePath}}} WHERE AssocClass = Win32_LogicalDiskToPartition");
                foreach (ManagementObject logical in query.Get())
                {
                    try
                    {
                        string fs = (logical["FileSystem"] as string) ?? "";
                        if (!string.IsNullOrEmpty(fs))
                            return fs;
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }
            catch
            {
                // ignore
            }

            return "";
        }

        private string GetDriveLetterForPartition(int diskNumber, uint partitionIndex)
        {
            using (var searcher = new ManagementObjectSearcher(
                       $"SELECT * FROM Win32_DiskPartition WHERE DiskIndex = {diskNumber} AND Index = {partitionIndex}"))
            {
                foreach (ManagementObject part in searcher.Get())
                {
                    try
                    {
                        var query = new ManagementObjectSearcher(
                            $"ASSOCIATORS OF {{{part.Path.RelativePath}}} WHERE AssocClass = Win32_LogicalDiskToPartition");

                        foreach (ManagementObject logical in query.Get())
                        {
                            string devId = (logical["DeviceID"] as string) ?? "";
                            if (!string.IsNullOrEmpty(devId))
                            {
                                // "E:" -> "E:\"
                                return devId.Trim() + "\\";
                            }
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                }
            }

            return null;
        }

        private void ExtendNtfsPartition(int diskNumber, uint partitionNumber)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"select disk {diskNumber}");
            sb.AppendLine($"select partition {partitionNumber}");
            sb.AppendLine("extend");
            RunDiskpartScript(sb.ToString());
        }

        private void CreateNewNtfsPartition(int diskNumber)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"select disk {diskNumber}");
            sb.AppendLine("create partition primary");
            sb.AppendLine("format fs=ntfs quick label=\"DATA\"");
            sb.AppendLine("assign");
            RunDiskpartScript(sb.ToString());
        }

        private void PrepareTargetDiskForClone(int diskNumber)
        {
            // ensure target disk is online, writable, and has no existing signatures/partitions
            var sb = new StringBuilder();
            sb.AppendLine($"select disk {diskNumber}");
            sb.AppendLine("online disk");
            sb.AppendLine("attributes disk clear readonly");
            sb.AppendLine("clean");

            AppendLog("Preparing target disk for clone (online + clear readonly + clean)...");
            RunDiskpartScript(sb.ToString());
        }

        private void RunDiskpartScript(string scriptContent)
        {
            string tempFile = Path.Combine(Path.GetTempPath(), "diskpart_usbcloner.txt");
            File.WriteAllText(tempFile, scriptContent);

            var startInfo = new ProcessStartInfo
            {
                FileName = "diskpart.exe",
                Arguments = $"/s \"{tempFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(startInfo))
            {
                if (proc == null) return;

                string output = proc.StandardOutput.ReadToEnd();
                string error = proc.StandardError.ReadToEnd();

                proc.WaitForExit();

                AppendLog("diskpart output:");
                AppendLog(output);

                if (!string.IsNullOrWhiteSpace(error))
                {
                    AppendLog("diskpart errors:");
                    AppendLog(error);
                }

                if (proc.ExitCode != 0)
                {
                    AppendLog($"diskpart exited with code {proc.ExitCode}.");
                }
            }

            try
            {
                File.Delete(tempFile);
            }
            catch
            {
                // ignore
            }
        }

        private void RunRobocopy(string srcRoot, string dstRoot, IProgress<int> progress, int startPercent, int endPercent)
        {
            // IMPORTANT: strip trailing backslashes so quotes are not broken: "D:" not "D:\"
            srcRoot = (srcRoot ?? "").TrimEnd('\\');
            dstRoot = (dstRoot ?? "").TrimEnd('\\');

            AppendLog($"Running robocopy from {srcRoot} to {dstRoot} ...");
            progress?.Report(startPercent);

            var psi = new ProcessStartInfo
            {
                FileName = "robocopy.exe",
                Arguments = $"\"{srcRoot}\" \"{dstRoot}\" /MIR /COPYALL /R:1 /W:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(psi))
            {
                if (proc == null)
                {
                    AppendLog("Failed to start robocopy.");
                    throw new Exception("Unable to start robocopy.");
                }

                // Stream output into our log
                while (!proc.HasExited)
                {
                    string line = proc.StandardOutput.ReadLine();
                    if (line == null) break;
                    AppendLog(line);
                }

                string remaining = proc.StandardOutput.ReadToEnd();
                if (!string.IsNullOrEmpty(remaining))
                    AppendLog(remaining);

                string err = proc.StandardError.ReadToEnd();
                if (!string.IsNullOrEmpty(err))
                {
                    AppendLog("robocopy errors:");
                    AppendLog(err);
                }

                proc.WaitForExit();
                AppendLog($"robocopy exit code: {proc.ExitCode}");

                // robocopy exit codes < 8 are normally "success with some noise"
                if (proc.ExitCode >= 8)
                {
                    throw new Exception($"robocopy failed with exit code {proc.ExitCode}.");
                }
            }

            progress?.Report(endPercent);
        }


        // (Capture/restore drive letters helpers kept for possible future use,
        // but no longer used in the current workflow.)

        private List<string> CaptureAndRemoveDriveLetters(int diskNumber)
        {
            var letters = new List<string>();

            using (var searcher = new ManagementObjectSearcher(
                       $"SELECT * FROM Win32_DiskPartition WHERE DiskIndex = {diskNumber}"))
            {
                var parts = searcher.Get().Cast<ManagementObject>()
                    .OrderBy(p => (ulong)p["StartingOffset"]);

                foreach (var part in parts)
                {
                    try
                    {
                        var assoc = new ManagementObjectSearcher(
                            $"ASSOCIATORS OF {{{part.Path.RelativePath}}} WHERE AssocClass = Win32_LogicalDiskToPartition");

                        foreach (ManagementObject logical in assoc.Get())
                        {
                            string devId = (logical["DeviceID"] as string) ?? "";
                            if (!string.IsNullOrEmpty(devId))
                            {
                                letters.Add(devId.Trim());
                            }
                        }
                    }
                    catch
                    {
                        // ignore odd partitions
                    }
                }
            }

            if (letters.Count == 0)
            {
                AppendLog("No drive letters found on target disk to remove.");
                return letters;
            }

            AppendLog("Temporarily removing drive letters from target disk: " + string.Join(", ", letters));

            var sb = new StringBuilder();
            foreach (var letter in letters.Distinct())
            {
                sb.AppendLine($"select volume {letter}");
                sb.AppendLine($"remove letter={letter}");
            }
            RunDiskpartScript(sb.ToString());

            return letters;
        }

        private void RestoreDriveLetters(int diskNumber, List<string> letters)
        {
            if (letters == null || letters.Count == 0)
            {
                AppendLog("No previous drive letters to restore.");
                return;
            }

            var parts = GetPartitions(diskNumber).OrderBy(p => p.StartingOffset).ToList();
            if (parts.Count == 0)
            {
                AppendLog("No partitions found when trying to restore letters.");
                return;
            }

            int max = Math.Min(parts.Count, letters.Count);
            var sb = new StringBuilder();

            AppendLog("Restoring drive letters on target disk...");

            for (int i = 0; i < max; i++)
            {
                string letter = letters[i];
                var part = parts[i];

                sb.AppendLine($"select disk {diskNumber}");
                sb.AppendLine($"select partition {part.Index}");
                sb.AppendLine($"assign letter={letter}");
            }

            RunDiskpartScript(sb.ToString());
        }

        private string FormatSize(long bytes)
        {
            double gb = bytes / (1024.0 * 1024 * 1024);
            return $"{gb:0.0} GB";
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Exit USB Build Stick Cloner?",
                                "Confirm Exit",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

    }
}
