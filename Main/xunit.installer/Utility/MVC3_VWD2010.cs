﻿using System.IO;
using System.Reflection;
using Microsoft.Win32;

namespace Xunit.Installer
{
    public class MVC3_VWD2010 : IApplication
    {
        const string TEMPLATE_NAME_CSHARP = "XunitMvc3TestProjectTemplate.cs.zip";
        const string TEMPLATE_NAME_VB = "XunitMvc3TestProjectTemplate.vb.zip";

        public bool Enableable
        {
            get
            {
                using (RegistryKey key = OpenMvc3Key())
                    return key != null;
            }
        }

        public bool Enabled
        {
            get
            {
                using (RegistryKey key = OpenMvc3Key())
                {
                    if (key == null)
                        return false;

                    using (RegistryKey templateKey = key.OpenSubKey("xUnit.net"))
                        return templateKey != null;
                }
            }
        }

        public string PreRequisites
        {
            get { return "Visual Web Developer 2010\r\nASP.NET MVC 3"; }
        }

        public string XunitVersion
        {
            get
            {
                using (RegistryKey key = OpenMvc3Key())
                using (RegistryKey csharpKey = key.OpenSubKey(@"xUnit.net\C#"))
                    return (string)csharpKey.GetValue("TestFrameworkName");
            }
        }

        public string Disable()
        {
            using (RegistryKey key = OpenMvc3Key())
                key.DeleteSubKeyTree("xUnit.net");

            UninstallZipFiles(VisualWebDeveloper2010.GetTestProjectTemplatePath("CSharp"));
            UninstallZipFiles(VisualWebDeveloper2010.GetTestProjectTemplatePath("VisualBasic"));

            return null;
        }

        public string Enable()
        {
            string CSharpPath = VisualWebDeveloper2010.GetTestProjectTemplatePath("CSharp");
            if (!Directory.Exists(CSharpPath))
                Directory.CreateDirectory(CSharpPath);

            ResourceHelper.WriteResourceToFile("Xunit.Installer.Templates.MVC3-CS-VS2010.zip",
                                               Path.Combine(CSharpPath, TEMPLATE_NAME_CSHARP));

            string VBPath = VisualWebDeveloper2010.GetTestProjectTemplatePath("VisualBasic");
            if (!Directory.Exists(VBPath))
                Directory.CreateDirectory(VBPath);

            ResourceHelper.WriteResourceToFile("Xunit.Installer.Templates.MVC3-VB-VS2010.zip",
                                               Path.Combine(VBPath, TEMPLATE_NAME_VB));

            using (RegistryKey key = OpenMvc3Key())
            using (RegistryKey templateKey = key.CreateSubKey("xUnit.net"))
            {
                using (RegistryKey csharpKey = templateKey.CreateSubKey("C#"))
                {
                    csharpKey.SetValue("AdditionalInfo", "http://xunit.codeplex.com/");
                    csharpKey.SetValue("Package", "");
                    csharpKey.SetValue("Path", @"CSharp\Test");
                    csharpKey.SetValue("Template", TEMPLATE_NAME_CSHARP);
                    csharpKey.SetValue("TestFrameworkName", "xUnit.net build " + Assembly.GetExecutingAssembly().GetName().Version);
                }
                using (RegistryKey vbKey = templateKey.CreateSubKey("VB"))
                {
                    vbKey.SetValue("AdditionalInfo", "http://xunit.codeplex.com/");
                    vbKey.SetValue("Package", "");
                    vbKey.SetValue("Path", @"VisualBasic\Test");
                    vbKey.SetValue("Template", TEMPLATE_NAME_VB);
                    vbKey.SetValue("TestFrameworkName", "xUnit.net build " + Assembly.GetExecutingAssembly().GetName().Version);
                }
            }

            return null;
        }

        static RegistryKey OpenMvc3Key()
        {
            return Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\VWDExpress\10.0\MVC3\TestProjectTemplates", true);
        }

        static void UninstallZipFiles(string templatePath)
        {
            foreach (string file in Directory.GetFiles(templatePath, "XunitMvc3TestProjectTemplate.*"))
                File.Delete(Path.Combine(templatePath, file));

            foreach (string directory in Directory.GetDirectories(templatePath))
                UninstallZipFiles(Path.Combine(templatePath, directory));
        }
    }
}