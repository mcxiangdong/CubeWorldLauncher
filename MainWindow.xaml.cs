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
using KMCCC.Authentication;
using KMCCC.Launcher;
using Panuon.UI.Silver;
using SquareMinecraftLauncher;
using SquareMinecraftLauncherWPF;
using KMCCC;
using KMCCC.Modules;
using KMCCC.Tools;
using KMCCC.Pro;

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
        SquareMinecraftLauncher.Minecraft.Tools tools = new SquareMinecraftLauncher.Minecraft.Tools();
        public static LauncherCore Core = LauncherCore.Create();
        public MainWindow()
        {
            InitializeComponent();

            //自动找版本
            var versions = Core.GetVersions().ToArray();
            versionCombo.ItemsSource = versions;//绑定数据源
            //自动找java
            List<string> javaList = new List<string>();
            foreach(string i in KMCCC.Tools.SystemTools.FindJava())
            {
                javaList.Add(i);
            }
            javaList.Add(tools.GetJavaPath());
            javaCombo.ItemsSource = javaList;
            //初始选择
            versionCombo.SelectedItem = versionCombo.Items[0];
            javaCombo.SelectedItem = javaCombo.Items[0];
        }
        public void GameStart()
        {
            LaunchOptions launchOptions = new LaunchOptions();
            switch (launchMode)
            {
                case 1:
                    launchOptions.Authenticator = new OfflineAuthenticator (OfflineLogin.IDText.Text);
                    break;
                case 2:
                    launchOptions.Authenticator = new YggdrasilLogin(MojangLogin.Email.Text,MojangLogin.password.Password,false);
                    break;
            }

            launchOptions.MaxMemory = Convert.ToInt32(MemoryTextbox.Text);
            if (OfflineLogin.IDText.Text !=string.Empty||(MojangLogin.Email.Text !=string.Empty&&MojangLogin.password.Password !=string.Empty)&&MemoryTextbox.Text != string.Empty&&versionCombo.Text != string.Empty && javaCombo.Text != string.Empty)
            try
            {
                Core.JavaPath = javaCombo.Text;
                var ver = (KMCCC.Launcher.Version)versionCombo.SelectedItem;
                launchOptions.Version = ver;

                var result = Core.Launch(launchOptions);
                //错误提示
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
                            MessageBoxX.Show(result.ErrorMessage, "出现错误");
                            break;
                        case ErrorType.Unknown:
                            MessageBoxX.Show("出现未知原因导致游戏启动失败，详细信息：" + result.ErrorMessage, "出现错误");
                            break ;
                    }
                }

            }
            catch
            {
                MessageBox.Show("貌似出现了一些问题,导致无法启动CWL", "出现错误");
            }
        }




        private void Button_Click(object sender, RoutedEventArgs e)
        {
            GameStart();

        }

        /// <summary>
        /// 离线登录
        /// <summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            {
                Content = OfflineLogin
            };
            launchMode = 1;
        }

        /// <summary>
        /// Mojang账户登录
        /// <summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            {
                Content = MojangLogin
            };
            launchMode = 2;
        }

        /// <summary>
        /// Microsoft账户登录
        /// <summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            ContentControl1.Content = new Frame
            {
                Content = MicrosoftLogin
            };
            launchMode = 3;
        }
    }
}

