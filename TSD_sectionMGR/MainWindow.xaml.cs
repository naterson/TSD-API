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
            public TSD.API.Remoting.Document.Document? Document { get; }
            public TSD.API.Remoting.Structure.Model? Model { get; }
            //public IEnumerable<TSD.API.Remoting.Structure.IMember?>? Members { get; }

            public ProjAttr(TSD.API.Remoting.Application instance, TSD.API.Remoting.Document.Document document, TSD.API.Remoting.Structure.Model model) //, IEnumerable<TSD.API.Remoting.Structure.IMember> members
            {
                Name = document.Path.ToString();
                IsSuccessful = true;
                Instance = instance;
                Document = document;
                Model = model;
                //Members = members;
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
            //var members = await model.GetMemberAsync(null);
            //if (!members.Any())
            //{
            //    return new ProjAttr("Error: No members found in model!");
            //}
            //IEnumerable<TSD.API.Remoting.Structure.IMember?>? members = null;
            return new ProjAttr(instance, document, model);
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
            string documentPath = docPath.Result;

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
                //var members = proj.Members;
                string str = "";
                //foreach(var member in members)
                //{
                //    str += member.Type.ToString() + "\n";
                //    Debug.WriteLine(str);
                //}

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

        private async void BRun_Click(object sender, RoutedEventArgs e)
        {
            OutputLog.Text += "\nStarted " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            // Get project attributes (instance, document, model)
            ProjAttr proj = GetProjAttr();
            var members = await proj.Model.GetMemberAsync(null);
            TSD.API.Remoting.Materials.MaterialType timberMatl = TSD.API.Remoting.Materials.MaterialType.Timber;
            TSD.API.Remoting.Materials.TimberFabrication glulamFab = TSD.API.Remoting.Materials.TimberFabrication.Glulam;
            TSD.API.Remoting.Sections.SectionType TBGL = TSD.API.Remoting.Sections.SectionType.GluedLaminatedTimberBeam;


            //TSD.API.Remoting.Sections.ISectionFactory sectionFactory = TSD.API.Remoting.Sections.ISectionFactory.
            
            foreach(var member in members)
            {
                var span = await member.GetSpanAsync();
                var spanLength = span.First().Length;
                var id = member.Id;
                var name = member.Name;
                var materialType = member.MaterialType.Value;
                var memberType = member.MemberType.Value;

                OutputLog.Text += ("\nMember Name: " + name.ToString() +
                                  "\n  ID: " + id.ToString() +
                                  "\n  Member Type: " + memberType.ToString() +
                                  "\n  Material: " + materialType.ToString() +
                                  "\n  Span Length: " + (spanLength/25.4).ToString() + "\"");

            }

            OutputLog.Text += "\nSections:\n";
            //var sections = proj.Model.SectionFactory.
            //sections.

            OutputLog.Text += "\nFinished " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}
