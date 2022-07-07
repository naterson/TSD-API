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
using System.Diagnostics;

using TSD.API;
using TSD.API.Remoting;

namespace TSD_sectionMGR
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // Get project instance, document, and model
            ProjAttr proj = GetProjAttr();
            ModelPath.Text = proj.Name;

            // Update Output Log
            OutputLog.Text += "\nInitialized " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            //OutputLog.Text += "\nModel: " + proj.Name;
        }

        public static async Task<string> GetDocumentPath()
        {

            // Get all instances of TSD running on the local machine
            var tsdRunningInstances = await ApplicationFactory.GetRunningApplicationsAsync();

            if (!tsdRunningInstances.Any())
            {
                return "No running instances of TSD found!";
            }

            // Get the first running TSD instance found
            var tsdInstance = tsdRunningInstances.First();

            // Get the active document from the running instance of TSD
            var document = await tsdInstance.GetDocumentAsync();

            if (document == null)
            {
                return "No document was found in the TSD instance!";
            }
            else
            {
                return document.Path;
            }
        } // OBSOLETE?
        public class ProjAttr
        {
            public string Name { get; }
            public bool IsSuccessful { get; }
            public TSD.API.Remoting.Application? Instance { get; }
            public TSD.API.Remoting.Document.IDocument? Document { get; }
            public TSD.API.Remoting.Structure.IModel? Model { get; }
            public IEnumerable<TSD.API.Remoting.Structure.IMember?>? Members { get; }

            public ProjAttr(TSD.API.Remoting.Application instance, TSD.API.Remoting.Document.IDocument document, TSD.API.Remoting.Structure.IModel model, IEnumerable<TSD.API.Remoting.Structure.IMember> members)
            {
                Name = document.Path.ToString();
                IsSuccessful = true;
                Instance = instance;
                Document = document;
                Model = model;
                Members = members;
            }

            public ProjAttr(string error)
            {
                Name = error;
                IsSuccessful = false;
            }
        }

        public static async Task<ProjAttr> MakeProjAttr()
        {
            var tsdRunningInstances = await ApplicationFactory.GetRunningApplicationsAsync();
            if (!tsdRunningInstances.Any())
            {
                return new ProjAttr("Error: No running instances of TSD found!");
            }
            var instance = tsdRunningInstances.First();
            var document = await instance.GetDocumentAsync();
            if (document == null)
            {
                return new ProjAttr("Error: No document found in TSD instance!");
            }
            var model = await document.GetModelAsync();
            if (model == null)
            {
                return new ProjAttr("Error: No model found in document!");
            }
            var members = await model.GetMemberAsync(null);
            if (!members.Any())
            {
                return new ProjAttr("Error: No members found in model!");
            }
            //IEnumerable<TSD.API.Remoting.Structure.IMember?>? members = null;
            return new ProjAttr(instance, document, model, members);
        }

        public static ProjAttr GetProjAttr()
        {
            Task<ProjAttr> projAttr = Task.Run(() => MakeProjAttr());
            projAttr.Wait();
            ProjAttr result = projAttr.Result;
            return result;
        }

        public static string TestTask()
        {
            Task<string> testTask = Task.Run(() => WriteSections());
            testTask.Wait();
            string result = testTask.Result;
            MessageBox.Show(result);
            return result;
        }

        public static Dictionary<string, string> GetParentDirectory()
        {
            // Get path to model
            Task<string> docPath = Task.Run(() => GetDocumentPath());
            docPath.Wait();
            string documentPath =  docPath.Result;

            // Make dictionary
            Dictionary<string, string> result = new Dictionary<string, string>();
            result.Add("Model", documentPath);

            string parent = documentPath.Substring(0, documentPath.LastIndexOf("\\"));
            result.Add("Parent", parent);

            return result;
        } // OBSOLETE?

        public static async Task<string> WriteSections() //TSD.API.Remoting.Sections.IExtendedSection
        {
            ProjAttr proj = GetProjAttr();
            if (proj.IsSuccessful == true)
            {
                var members = proj.Members;
                string str = "";
                foreach(var member in members)
                {
                    str += member.Type.ToString() + "\n";
                    Debug.WriteLine(str);
                }

                return str;
            }
            else { return "Error"; };
            


        }

        private void BBrowse_Click(object sender, RoutedEventArgs e)
        {
            // Get filepath and parent directory address
            Dictionary<string, string> paths = GetParentDirectory();
            string defaultSectionDataPath = paths["Parent"];

            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            openFileDlg.InitialDirectory = defaultSectionDataPath;
            bool? result = openFileDlg.ShowDialog();
            if (result == true)
            {
                string sectionDataPath = openFileDlg.FileName;
                SectionDataPath.Text = sectionDataPath;
                OutputLog.Text += "\nSections to add: " + sectionDataPath;
            }
        }

        private void BRun_Click(object sender, RoutedEventArgs e)
        {
            OutputLog.Text += "\nStarted " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Get project attributes (instance, document, model)
            ProjAttr proj = GetProjAttr();
            if(proj.IsSuccessful == true)
            {
                var members = proj.Members;

                foreach(var member in members)
                {
                    OutputLog.Text += "hi" + "\n";
                }
            }


            OutputLog.Text += "\nFinished " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
