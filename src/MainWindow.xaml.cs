using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Panuon.UI.Silver;
using SquareMinecraftLauncher;
using SquareMinecraftLauncherWPF;
using KMCCC.Launcher;
using KMCCC.Authentication;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.ComponentModel;
using Microsoft.Win32;

namespace Cube_World_Launcher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : WindowX
    {
        //引入本地
        LoginUI.OfflineLogin OfflineLogin = new LoginUI.OfflineLogin();
        LoginUI.MicrosoftLogin MicrosoftLogin = new LoginUI.MicrosoftLogin();
        LoginUI.MojangLogin MojangLogin = new LoginUI.MojangLogin();
        public int launchMode = 1;
        SquareMinecraftLauncher.Minecraft.Game game = new SquareMinecraftLauncher.Minecraft.Game();
        SquareMinecraftLauncher.Minecraft.Tools tools = new SquareMinecraftLauncher.Minecraft.Tools();
        SquareMinecraftLauncher.MinecraftDownload minecraftDownload = new SquareMinecraftLauncher.MinecraftDownload();
        //创建"CWLSetting.json"，并将其作为配置文件
        string settingPath = @"CWLSetting.json";
        Setting setting = new Setting();
        RegSetting regSetting = new RegSetting();
        //加载KMCCC核心
        public static LauncherCore Core = LauncherCore.Create();
        
        //数据保存标识
        public class Setting
        {
            public string Ram = "默认值";
        }
        public class RegSetting
        {
            public string name = "默认值";
        }

        //数据保存主代码集
        public void LauncehrInitialization()
        {
            
            if (!File.Exists(settingPath))
            {
                File.WriteAllText(settingPath, JsonConvert.SerializeObject(setting));
            }
            else
            {
                setting = JsonConvert.DeserializeObject<Setting>(File.ReadAllText(settingPath));
            }
            bool isFirst=false;
            using(RegistryKey key1 = Registry.LocalMachine.OpenSubKey("SOFTWARE"))
            {
                foreach(var i in key1.GetValueNames())
                {
                    if(i == "CWL")
                    {
                        isFirst=true;
                    }
                }
            }
            if (isFirst)
            {
            using (RegistryKey key = Registry.LocalMachine)
            {
                using (RegistryKey software = key.CreateSubKey("software\\CWL"))
                {
                        software.SetValue("AccountName", regSetting.name);
                }
                }
            }
            else
            {
                using (RegistryKey key = Registry.LocalMachine)
                {
                    using (RegistryKey software = key.CreateSubKey("software\\CWL"))
                    {
                        regSetting.name = software.GetValue("AccountName").ToString();
                    }
                }
            }
            OfflineLogin.IDText.Text = regSetting.name;
            //自动找版本
            var versions = tools.GetAllTheExistingVersion();
            versionCombo.ItemsSource = versions;//绑定数据源
            //自动找java
            List<string> javaList = new List<string>();
            foreach (string i in KMCCC.Tools.SystemTools.FindJava())
            {
                javaList.Add(i);
            }
            javaList.Add(tools.GetJavaPath());
            javaCombo.ItemsSource = javaList;
            //自动选择
            if (versionCombo.Items.Count != 0)
                versionCombo.SelectedItem = versionCombo.Items[0];
            if (versionCombo.Items.Count != 0)
                javaCombo.SelectedItem = javaCombo.Items[0];
            //应用读取CWLSetting.json得到的数值
            MemoryTextbox.Text = setting.Ram;
        }
        //主窗口
        public MainWindow()
        {
            InitializeComponent();
            LauncehrInitialization();
            ServicePointManager.DefaultConnectionLimit = 512;
            
        }
        
        //开始游戏的代码
        public void GameStart()
        {
            LaunchOptions launchOptions = new LaunchOptions();
            switch (launchMode)
            {
                case 1:
                    launchOptions.Authenticator = new OfflineAuthenticator(OfflineLogin.IDText.Text);
                    break;
                case 2:
                    launchOptions.Authenticator = new YggdrasilLogin(MojangLogin.Email.Text, MojangLogin.password.Password, false);
                    break;

            }

            launchOptions.MaxMemory = Convert.ToInt32(MemoryTextbox.Text);
            if (versionCombo.Text != string.Empty &&javaCombo.Text != string.Empty &&(OfflineLogin.IDText.Text != string.Empty || (MojangLogin.Email.Text != string.Empty && MojangLogin.password.Password != string.Empty) &&MemoryTextbox.Text != string.Empty))
            {
                try
                {
                    if (launchMode != 3)
                    {
                        Core.JavaPath = javaCombo.Text;
                        var ver = (KMCCC.Launcher.Version)versionCombo.SelectedItem;
                        launchOptions.Version = ver;

                        var result = Core.Launch(launchOptions);
                        if (!result.Success)
                        {
                            switch (result.ErrorType)
                            {
                                case ErrorType.NoJAVA:
                                    MessageBoxX.Show("我们貌似没有在你的计算机上找到JAVA...请检查JAVA后继续,详细信息：" + result.ErrorMessage, "出现错误");
                                    break;
                                case ErrorType.AuthenticationFailed:
                                    MessageBoxX.Show("你的账户名是无效字符或登录程序出错...详细信息：" + result.ErrorMessage, "出现错误");
                                    break;
                                case ErrorType.UncompressingFailed:
                                    MessageBoxX.Show("你的Minecraft文件似乎不翼而飞了,请检查Minecraft文件完整性,详细信息：" + result.ErrorMessage, "出现错误");
                                    break;
                                default:
                                    MessageBoxX.Show(result.ErrorMessage, "错误");
                                    break;
                            }
                        }
                    }



                }
                catch
                {
                    MessageBoxX.Show("貌似出现了一些问题,导致无法启动游戏...", "出现错误");
                }
            }
            else
            {
                MessageBoxX.Show("你的信息未填完整，无法初始化Minecraft，请检查你的信息填入情况...", "出现错误");
            }
        }

        //开始游戏的按钮事件
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GameStart();

        }

        //离线登录
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            {
                Content = OfflineLogin
            };
            launchMode = 1;
        }

        //MOJANG账户登录
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            {
                Content = MojangLogin
            };
            launchMode = 2;
        }

        //微软账户登录
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            {
                Content = MicrosoftLogin
            };
            launchMode = 3;
        }

        //读取CWLSetting.json
        private void MemoryTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            setting.Ram = MemoryTextbox.Text;
        }
        //当窗口关闭时，把账户信息储存到注册表里
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            File.WriteAllText(settingPath, JsonConvert.SerializeObject(setting));
            using (RegistryKey key = Registry.LocalMachine)
            {
                using (RegistryKey software = key.CreateSubKey("software\\CWL"))
                {
                    software.SetValue("AccountName", OfflineLogin.IDText.Text);
                }
            }
        }
    }
}

