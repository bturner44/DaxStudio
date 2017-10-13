﻿using System;
using System.Management;
using System.Threading;

namespace DaxStudio.Checker
{
    public static class SystemInfo
    {
        public static void OutputOSInfo(this System.Windows.Controls.RichTextBox output)
        {
            var osInfo = GetOSInfo();
            output.AppendLine($"OSCaption       = {osInfo.Name}");
            output.AppendLine($"OSVersion       = {osInfo.Version.ToString()}");
            output.AppendLine($"OSArchitecture  = {osInfo.Architecture}");
            output.AppendLine($"VisibleMemoryGB = {osInfo.TotalVisibleMemory.ToString("n2")}");
            output.AppendLine($"FreeMemoryGB    = {osInfo.TotalFreeMemory.ToString("n2")}");

        }

        public static void OutputCultureInfo(this System.Windows.Controls.RichTextBox output)
        {
            var curCulture = Thread.CurrentThread.CurrentCulture;
            output.AppendLine($"Culture Name              = {curCulture.Name}");
            output.AppendLine($"Culture DisplayName       = {curCulture.DisplayName}");
            output.AppendLine($"Culture EnglishName       = {curCulture.EnglishName}");
            output.AppendLine($"Culture 2-Letter ISO Name = {curCulture.TwoLetterISOLanguageName}");
            output.AppendLine($"Culture DecimalSeparator  = {curCulture.NumberFormat.NumberDecimalSeparator}");
            output.AppendLine($"Culture GroupSeparator    = {curCulture.NumberFormat.NumberGroupSeparator}");
            output.AppendLine($"Culture CurrencySymbol    = {curCulture.NumberFormat.CurrencySymbol}");
            output.AppendLine($"Culture ShortDatePattern  = {curCulture.DateTimeFormat.ShortDatePattern}");


        }

        private static OSInfo GetOSInfo()
        {

            ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
            var result = new OSInfo();
            foreach (ManagementObject os in searcher.Get())
            {
                result.Name = os["Caption"].ToString();
                result.Version = Version.Parse(os["Version"].ToString());
                result.Architecture = "32 bit";
                if (result.Version.Major > 5) result.Architecture = os["OSArchitecture"].ToString();
                continue;
            }

            searcher = new ManagementObjectSearcher("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM Win32_OperatingSystem");
            foreach (ManagementObject os in searcher.Get())
            {
                result.TotalVisibleMemory = long.Parse(os["TotalVisibleMemorySize"].ToString()).KbToGb();
                result.TotalFreeMemory = long.Parse(os["FreePhysicalMemory"].ToString()).KbToGb();

            }
            return result;

        }

        private struct OSInfo
        {
            public string Name;
            public Version Version;
            public string Architecture;
            public decimal TotalVisibleMemory;
            public decimal TotalFreeMemory;
        }

        public static decimal KbToGb(this long bytes)
        {
            return (decimal)bytes / (1024 * 1024);
        }
    }

}