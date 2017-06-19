//andywm, 2017, UoH 08985 ACW1
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Microsoft.Win32;

namespace TrustworthyACW1.user_interface
{
    public class GUI
    {
        //----------------------------------------------------------------------
        //----------Class Attribute Declarations--------------------------------
        //----------------------------------------------------------------------

        private SandboxWindow mWindow;
        private List<Aggregate> mAggregation;
        public Aggregate arguments { get; internal set; } = new Aggregate();
        public Aggregate path { get; internal set; } = new Aggregate();
        public Aggregate jail { get; internal set; } = new Aggregate();
        public bool terminate { get; internal set; }

        public bool supress { get; internal set; }
        public bool keepTerminalAlive { get; internal set; }

        //----------------------------------------------------------------------
        //----------Implementation Code-----------------------------------------
        //----------------------------------------------------------------------

        public GUI(List<Aggregate> agg)
        {
            mAggregation = agg;
            Thread t = new Thread(manageWindow);
            t.SetApartmentState(ApartmentState.STA);
            t.Start();

            t.Join(); //wait
        }
 
        [STAThread]
        private void manageWindow()
        {
            mWindow = new SandboxWindow(mAggregation, this);
            mWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            mWindow.ShowDialog();

            //keep thread alive until window closes.
            while (mWindow.IsActive) { } 
        }
    }
    internal class SandboxWindow : Window
    {
        //----------------------------------------------------------------------
        //----------Class Attribute Declarations--------------------------------
        //----------------------------------------------------------------------
        
        private Dictionary<UIElement, UIElement> mUIAssociations =
                new Dictionary<UIElement, UIElement>();
        private Dictionary<UIElement, Aggregate> mUIToAggregation =
                new Dictionary<UIElement, Aggregate>();

        private Aggregate mProgramArgumentList = new Aggregate();
        private Aggregate mProgramPath = new Aggregate();
        private Aggregate mProgramJail = new Aggregate();
        private string mPathEnvVar;
        private GUI mManager;
        private OpenFileDialog mFileDialog_Open = new OpenFileDialog();
        private SaveFileDialog mFileDialog_Save = new SaveFileDialog();
        private DockPanel mControlsLayout = new DockPanel();
        private DockPanel mLayoutPane = new DockPanel();
        private Grid mConfigurationPane = new Grid();
        private Grid mMainControlPane = new Grid();
        private Grid mTreePane = new Grid();
        private TreeView mTree = new TreeView();            

        //----------------------------------------------------------------------
        //----------Implementation Code-----------------------------------------
        //----------------------------------------------------------------------

        /// <summary>
        /// Constructs Window, generates UI components, and configures 
        /// attributes which are used for later reference.
        /// </summary>
        /// <param name="options"></param>
        /// <param name="manager"></param>
        public SandboxWindow(List<Aggregate> options, GUI manager)
        {
            mManager = manager;
            manager.terminate = true; //default to terminate on window close. 

            /***Auto-generate user interface***/
            configureTopLevelFrames();

            foreach (var option in options)
            {
                //Generate Base Option
                var baseOpt = genTreeElement(option);

                //Generate Control Tabs
                var controls = genTopLevelFrame(option.aggregation);

                baseOpt.AddHandler(TreeViewItem.SelectedEvent,
                    new RoutedEventHandler(topLevelViewChanged));
                mTree.Items.Add(baseOpt);

                mUIAssociations.Add(baseOpt, controls);
            }
            mMainControlPane.Children.Add(genConstantPanel());
            /***-----------------------------***/

            //select treeview default.
            ((TreeViewItem)mTree.Items[0]).IsSelected = true;
        }

        //----------------------------------------------------------------------
        //----------UI Generation Code------------------------------------------
        //----------------------------------------------------------------------

        /// <summary>
        /// Correctly associates top level objects with each other, and 
        /// sets their initial formating.
        /// </summary>
        private void configureTopLevelFrames()
        {
            mTreePane.MinWidth = 100.0;

            //set up frame hierarchy
            AddChild(mLayoutPane);
            mLayoutPane.Children.Add(mTreePane);
            mLayoutPane.Children.Add(mControlsLayout);
            mTreePane.Children.Add(mTree);

            //Docking style for frames.
            mControlsLayout.VerticalAlignment = VerticalAlignment.Stretch;
            DockPanel.SetDock(mMainControlPane, Dock.Bottom);
            mControlsLayout.Children.Add(mMainControlPane);
            DockPanel.SetDock(mConfigurationPane, Dock.Top);
            mControlsLayout.Children.Add(mConfigurationPane);
        }

        /// <summary>
        /// Generates user controls for execution control.
        /// </summary>
        /// <returns></returns>
        private UIElement genConstantPanel()
        {
            //this method is awful. Break it up, or find a way to use the
            //existing generators.

            //define UI Elements.
            StackPanel sp_master = new StackPanel(), sp_opts = new StackPanel();
            StackPanel sp_path = new StackPanel(), sp_args = new StackPanel();
            StackPanel sp_jail = new StackPanel();
            DockPanel dp_file_open_x = new DockPanel();
            DockPanel dp_file_open_j = new DockPanel();
            Button btn_ofd_x = new Button(), btn_ofd_j = new Button();
            Button btn_exe = new Button(), btn_cfg = new Button();                  
            TextBox tbx_p = new TextBox(), tbx_a = new TextBox();
            TextBox tbx_j = new TextBox();
            Label lbl_px = new Label(), lbl_a = new Label();
            Label lbl_pj = new Label();
            UniformGrid grd_buttons = new UniformGrid();
            CheckBox cb_sup = new CheckBox(), cb_alive = new CheckBox();

            //define UI element hierarchy.
            sp_master.Orientation = Orientation.Vertical;
            sp_path.Orientation = Orientation.Vertical;
            sp_args.Orientation = Orientation.Vertical;
            sp_jail.Orientation = Orientation.Vertical;

            sp_master.Children.Add(sp_opts);
            sp_master.Children.Add(sp_path);
            //sp_master.Children.Add(sp_jail); //doesn't work.
            sp_master.Children.Add(sp_args);
            sp_master.Children.Add(grd_buttons);
                  
            sp_path.Children.Add(lbl_px);
            sp_path.Children.Add(dp_file_open_x);

            sp_jail.Children.Add(lbl_pj);
            sp_jail.Children.Add(dp_file_open_j);

            sp_opts.Children.Add(cb_alive);
            sp_opts.Children.Add(cb_sup);
            
            DockPanel.SetDock(btn_ofd_x, Dock.Right);
            dp_file_open_x.Children.Add(btn_ofd_x);
            dp_file_open_x.Children.Add(tbx_p);

            DockPanel.SetDock(btn_ofd_j, Dock.Right);
            dp_file_open_j.Children.Add(btn_ofd_j);
            dp_file_open_j.Children.Add(tbx_j);

            sp_args.Children.Add(lbl_a);
            sp_args.Children.Add(tbx_a);

            grd_buttons.Children.Add(btn_exe);
            grd_buttons.Children.Add(btn_cfg);

            //label elements.
            btn_exe.Content = "Execute";
            btn_cfg.Content = "Generate Config";
            btn_ofd_x.Content = "Open";
            btn_ofd_j.Content = "Open";
            lbl_px.Content = "Program Path";
            lbl_pj.Content = "Chroot Jail";
            lbl_a.Content = "Program Arguments";
            cb_alive.Content = "Keep Console Alive";
            cb_sup.Content = "Supress Sandbox Messages";

            //event callbacks
            mUIAssociations.Add(btn_ofd_x, tbx_p);
            mUIAssociations.Add(btn_ofd_j, tbx_j);
            mUIToAggregation.Add(tbx_a, mProgramArgumentList);
            mUIToAggregation.Add(tbx_p, mProgramPath);
            mUIToAggregation.Add(tbx_j, mProgramJail);
            btn_exe.AddHandler(Button.ClickEvent,
                            new RoutedEventHandler(onExecutionPressed));
            btn_cfg.AddHandler(Button.ClickEvent,
                           new RoutedEventHandler(onConfigPressed));
            btn_ofd_x.AddHandler(Button.ClickEvent,
                           new RoutedEventHandler(onOpenFile));
            btn_ofd_j.AddHandler(Button.ClickEvent,
                         new RoutedEventHandler(onOpenFile));
            tbx_p.AddHandler(TextBox.TextChangedEvent,
                           new RoutedEventHandler(onStringInput));
            tbx_a.AddHandler(TextBox.TextChangedEvent,
                           new RoutedEventHandler(onStringInput));
            cb_alive.AddHandler(CheckBox.CheckedEvent,
                           new RoutedEventHandler(onRetain));
            cb_sup.AddHandler(CheckBox.CheckedEvent,
                           new RoutedEventHandler(onSupress));

            return sp_master;
        }

        /// <summary>
        /// Responsible for ensure mutually exlusive option sets
        /// are displayed in seperate tabs.
        /// </summary>
        /// <param name="aggs"></param>
        /// <returns></returns>
        private UIElement genTopLevelFrame(List<Aggregate> aggs)
        {
            //generate compositor UI element.
            TabControl tabs = new TabControl();
            int count=0;

            //generate all ui components.
            foreach (var aggregate in aggs)
            {
                if (aggregate.aggregation != null &&
                    aggregate.aggregation.Count > 0)
                {
                    //generate subcomponents.
                    var opts = genFrame(aggregate.aggregation);

                    TabItem tbi = new TabItem();
                    if (count == 0)
                    { 
                        tbi.Header = "Coarse Configuration";
                        count++;
                    }
                    else
                        tbi.Header = "Granular Configuration";

                    tbi.Content = opts;

                    //create event mapping.
                    mUIToAggregation.Add(tbi, aggregate);
                    tabs.AddHandler(TabControl.SelectionChangedEvent,
                            new RoutedEventHandler(onSetChange));

                    //default.
                    tabs.SelectedIndex = 0;

                    //add to compositor.
                    tabs.Items.Add(tbi);              
                }
            }
            return tabs;
        }
   
        /// <summary>
        /// Responsible for recursively generating UI components
        /// based on information contained in the aggregate.
        /// </summary>
        /// <param name="aggs"></param>
        /// <returns></returns>
        private StackPanel genFrame(List<Aggregate> aggs)
        {
            //generate UI Compositor elements.
            StackPanel stk = new StackPanel();
            stk.Orientation = Orientation.Vertical;

            //generate sub components.
            foreach (var aggregation in aggs)
            {
                //using associated metadata, generate correct element or
                //recurse until doing so is possible.
                switch (aggregation.handleAs)
                {
                    case Aggregate.HANDLE_AS.CLASS:
                        stk.Children.Add(genFrame(aggregation.aggregation));
                        break;
                    case Aggregate.HANDLE_AS.ENUM:
                        stk.Children.Add(genEnumerationSelector(aggregation));
                        break;       
                    case Aggregate.HANDLE_AS.SYSCLASS:
                        stk.Children.Add(genInputForSysClass(aggregation));
                        break;
                }  
            }
            return stk;
        }

        /// <summary>
        /// Responsible for generating both modal and non modal
        /// enumerated UI components.
        /// </summary>
        /// <param name="agg"></param>
        /// <returns></returns>
        private UIElement genEnumerationSelector(Aggregate agg)
        {
            //gen base ui elements.
            StackPanel stk = new StackPanel();
            ComboBox cbx = new ComboBox();
            UniformGrid grd = new UniformGrid();
            Label lbl = new Label();
            lbl.Content = agg.name;

            //determine the nature of modallity.
            bool modal = true;
            for (int i= 0; i< agg.aggregation.Count; i++)
            {
                //why is the C# for iterator const? Its annoying...
                var item = agg.aggregation[i];

                //mutal exclusion in this case means all other options should be
                //deselected if this one is choosen. i.e. AllFlags and NoFlags 
                //are a direct contradiction.

                //modality is whether ALL options are mutually exclusive.
                if (item.name == "AllFlags")
                {
                    modal = false;
                    item.isMutuallyExclusive = true;
                }
                else if (item.name == "NoFlags")
                {
                    item.isMutuallyExclusive = true;
                }           
            }

            //add to compositor.
            stk.Children.Add(lbl);

            //deal with modality.
            if (modal)
            {
                //for modal, generate a list of mutually exclusive 
                //options, i.e only one may be set.
                foreach (var item in agg.aggregation)
                {
                    cbx.Items.Add(item.name);
                }

                //add to compositor.
                stk.Children.Add(cbx);
                
                //generate event aggregate mappings, and event callbacks.
                mUIToAggregation.Add(cbx, agg);
                cbx.AddHandler(ComboBox.SelectionChangedEvent,
                            new RoutedEventHandler(onSingleSelect));

                //select default.
                cbx.SelectedIndex = 0;
            }
            else
            {
                List<ToggleButton> tbnList = new List<ToggleButton>();
                foreach (var item in agg.aggregation)
                {
                    ToggleButton btn = new ToggleButton();
                    btn.Content = item.name;
                    grd.Children.Add(btn);
                    tbnList.Add(btn);

                    //generate event aggregate mappings, and event callbacks.
                    //(this is per button)
                    mUIToAggregation.Add(btn, agg);
                    btn.AddHandler(Button.ClickEvent,
                        new RoutedEventHandler(onMultiSelect));
                }
                //select default.
                tbnList[0].IsChecked = true;

                stk.Children.Add(grd);
                
            }
            
            return stk;
        }
    
        /// <summary>
        /// Responsible generating tree view elements.
        /// </summary>
        /// <param name="agg"></param>
        /// <returns></returns>
        private UIElement genTreeElement(Aggregate agg)
        {
            //generate UI Elements.
            TreeViewItem tvi = new TreeViewItem();
            tvi.Header = agg.name;
            //this is absolutely where tvi events should be set up.
            return tvi;
        }

        /// <summary>
        /// Responsible for generating UI components which deal with aggregates
        /// classed as SysClass : i.e. string, bool, int...
        /// </summary>
        /// <param name="agg"></param>
        /// <returns></returns>
        private UIElement genInputForSysClass(Aggregate agg)
        {
            if (agg.type == typeof(string) || agg.type == typeof(int))
            {
                //generate UI Elements.
                StackPanel stk = new StackPanel();
                Label lbl = new Label();
                lbl.Content = agg.name;
                TextBox tbx = new TextBox();

                //add to compositional tree.
                stk.Children.Add(lbl);
                stk.Children.Add(tbx);

                //establish mappings and callbacks.
                mUIToAggregation.Add(tbx, agg);
                tbx.AddHandler(TextBox.TextChangedEvent,
                        new RoutedEventHandler(onStringInput));
                //note: this event is inefficient as the update will occur for
                //each keystroke. 

                //default
                tbx.Text = "";

                return stk;
            }
            else if (agg.type == typeof(bool))
            {
                //generate UI Elements.
                CheckBox check = new CheckBox();
                check.Content = agg.name;

                //establish mappings and callbacks.
                mUIToAggregation.Add(check, agg);
                check.AddHandler(CheckBox.CheckedEvent,
                        new RoutedEventHandler(onBooleanInput));

                //default.
                check.IsChecked = false;
                return check;
            }
            else return new Label(); //fix this.
        }

        //----------------------------------------------------------------------
        //---------------Utilities----------------------------------------------
        //----------------------------------------------------------------------

        /// <summary>
        /// Responsible for determining whether the current selection
        /// contradicts the mutually exlusive flag.
        /// 
        /// Such that no options may be set where any option has a
        /// differing mutual exlusion flag, or where more than one 
        /// option has a mutal exclusion flag set.
        /// </summary>
        /// <param name="agg"></param>
        /// <param name="inPane"></param>
        /// <returns></returns>
        private bool isContradiction(Aggregate agg, Panel inPane)
        {
            int count0 = 0;
            int count1 = 0;
            var children = inPane.Children;
            for (int x = 0; x < children.Count; x++)
            {
                var child = children[x];

                if (((ToggleButton)child).IsChecked == true)
                {
                    foreach (var a in agg.aggregation)
                    {
                        if (a.name == (string)((ToggleButton)child).Content)
                        {
                            if (a.isMutuallyExclusive) count1++;
                            else count0++;
                        }
                    }
                }
            }

            if (count1 > 1 || (count0 > 0 && count1 > 0))
                return true;
            return false;
        }

        //----------------------------------------------------------------------
        //----------UI Event Code-----------------------------------------------
        //----------------------------------------------------------------------

        /// <summary>
        /// Responsible for changing the control pane to reflection the controls
        /// that should be present for a particular tree view element.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void topLevelViewChanged(object sender, RoutedEventArgs e)
        {
            mConfigurationPane.Children.Clear();
            mConfigurationPane.Children.Add(mUIAssociations[(UIElement)sender]);
        }

        /// <summary>
        /// Ensures user input for boolean operations is propogated
        /// to aggregate data structure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onBooleanInput(object sender, RoutedEventArgs e)
        {
            var agg = mUIToAggregation[(UIElement)sender];
            agg.value.bData = (bool)((CheckBox)sender).IsChecked;
        }

        /// <summary>
        /// Ensures user input for string operations is propogated
        /// to aggregate data structure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onStringInput(object sender, RoutedEventArgs e)
        {
            var agg = mUIToAggregation[(UIElement)sender];

            agg.value.sData = ((TextBox)sender).Text;
        }

        /// <summary>
        /// Ensures user input for multi-selection enumeration operations
        /// are propogated to aggregate data structure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onMultiSelect(object sender, RoutedEventArgs e)
        {
            var agg = mUIToAggregation[(UIElement)sender];

            Panel pane = (Panel)((ToggleButton)sender).Parent;
            string latest = (string)((ToggleButton)sender).Content;
            bool contradiction = isContradiction(agg, pane);

            //set correct options.
            var children = pane.Children;
            for (int i = 0; i < children.Count; i++)
            {
                var child = children[i];

                //note: the order of agg.aggregation is implictly the 
                //same as pane.children, as the latter was generated
                //from the fromer.
                if ((string)((ToggleButton)child).Content == latest)
                {
                    agg.aggregation[i].value.bData = true;
                }
                else if(contradiction)
                {
                    ((ToggleButton)child).IsChecked = false;
                    agg.aggregation[i].value.bData = false;
                }                  
            }
        }

        /// <summary>
        /// Ensures user input for single enumeration operations are
        /// propogated to aggregate data structure.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onSingleSelect(object sender, RoutedEventArgs e)
        {
            var agg = mUIToAggregation[(UIElement)sender];
            Aggregate.EventData cEvent;

            //unset all elements associated with UI component, then
            //when on the correct element. Set it.
            for(int i = 0; i < agg.aggregation.Count; i++)
            {
                var cAgg = agg.aggregation[i];
                cEvent = cAgg.value;
                cEvent.bData = false;

                if (cAgg.name == (string)((ComboBox)sender).SelectedValue)
                {
                    cEvent.bData = true;
                }
            }
        }

        /// <summary>
        /// Changes the working configuration set to reflect the tree view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onSetChange(object sender, RoutedEventArgs e)
        {
            var tabs = (TabControl)sender;
            var cTab = (TabItem)tabs.SelectedItem;

            foreach(var tab in tabs.Items)
            {
                var aggr = mUIToAggregation[(TabItem)tab];
                aggr.value.bData = false;
            }

            var agg = mUIToAggregation[cTab];
            agg.value.bData = true;
        }

        /// <summary>
        /// Propogate variables to out fields, sets in progress execution.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onExecutionPressed(object sender, RoutedEventArgs e)
        {
            mManager.arguments = mProgramArgumentList;
            mManager.path = mProgramPath;
            mManager.terminate = false;

            Close(); //close the window...
        }

        /// <summary>
        /// Invokes the saving process for a configuration file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onConfigPressed(object sender, RoutedEventArgs e)
        {
                MessageBox.Show("Feaure not implemented.",
                    "Warning",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
        }

        /// <summary>
        /// Opens file dialog to locate a file. Sets class attribute.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onOpenFile(object sender, RoutedEventArgs e)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            FileDialogCustomPlace fdcp = new FileDialogCustomPlace(dir);
            mFileDialog_Open.CustomPlaces.Add(fdcp);

            TextBox tbx = ((TextBox)mUIAssociations[(UIElement)sender]);
            Nullable<bool> result = mFileDialog_Open.ShowDialog();
            if (result == true)
                tbx.Text = mFileDialog_Open.FileName;
        }

        /// <summary>
        /// Opens file dialoge to save out a file, sets attribute.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void onSaveFile(object sender, RoutedEventArgs e)
        {
            string dir = AppDomain.CurrentDomain.BaseDirectory;
            FileDialogCustomPlace fdcp = new FileDialogCustomPlace(dir);
            mFileDialog_Save.CustomPlaces.Add(fdcp);

            Nullable<bool> result = mFileDialog_Save.ShowDialog();
            if (result == true)
                mPathEnvVar = mFileDialog_Save.FileName;
        }

        private void onSupress(object sender, RoutedEventArgs e)
        {
            mManager.supress = (bool)((CheckBox)sender).IsChecked;
        }

        private void onRetain(object sender, RoutedEventArgs e)
        {
            mManager.keepTerminalAlive = (bool)((CheckBox)sender).IsChecked;
        }
    }
}
//andywm, 2017, UoH 08985 ACW1
