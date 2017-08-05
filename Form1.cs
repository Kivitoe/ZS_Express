using ScintillaNET;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZS_Express
{
    public partial class Form1 : Form
    {
        int lastCaretPos = 0;
        public NewScript ns;
        int scripts = 0;



        public Form1(string path)
        {
            InitializeComponent();
            if (path != string.Empty)
            {
                scintilla.Text = File.ReadAllText(path);
            }
            
                
        }

        private void iewToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            treeView1.HideSelection = false; // Keep the list item selected (Makes saving/closing much easier)


            // Scintilla Options bellow.
            scintilla.Lexer = Lexer.Cpp;

            scintilla.StyleResetDefault();
            scintilla.Styles[Style.Default].Font = "Consolas";
            scintilla.Styles[Style.Default].Size = 10;
            scintilla.StyleClearAll();

            // Configure the ZScript lexer styles
            scintilla.IndentationGuides = IndentView.LookBoth; // Show indentations.

            scintilla.Styles[Style.Cpp.Default].ForeColor = Color.Black;
            scintilla.Styles[Style.Cpp.Comment].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[Style.Cpp.CommentLine].ForeColor = Color.FromArgb(0, 128, 0); // Green
            scintilla.Styles[Style.Cpp.CommentLineDoc].ForeColor = Color.FromArgb(128, 128, 128); // Gray
            scintilla.Styles[Style.Cpp.Number].ForeColor = Color.Olive;
            scintilla.Styles[Style.Cpp.Word].ForeColor = Color.Blue;
            scintilla.Styles[Style.Cpp.Word2].ForeColor = Color.Blue;
            scintilla.Styles[Style.Cpp.String].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla.Styles[Style.Cpp.Character].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla.Styles[Style.Cpp.Verbatim].ForeColor = Color.FromArgb(163, 21, 21); // Red
            scintilla.Styles[Style.Cpp.StringEol].BackColor = Color.Pink;
            scintilla.Styles[Style.Cpp.Operator].ForeColor = Color.Purple;
            scintilla.Styles[Style.Cpp.Preprocessor].ForeColor = Color.Maroon;
            scintilla.Styles[Style.BraceLight].BackColor = Color.LightGray;
            scintilla.Styles[Style.BraceLight].ForeColor = Color.BlueViolet;
            scintilla.Styles[Style.BraceBad].ForeColor = Color.Red;

            // Keywords to identify in ZScript
            scintilla.SetKeywords(0, "do if for while true false else null this ffc item global switch case default break continue import Link Game Screen return");
            scintilla.SetKeywords(1, "link screen game ffc npc lweapon eweapon item itemdata bool byte char const decimal double enum float int long short static struct void script");

            scintilla.Margins[0].Width = 16;



            // Code folding for voids, scripts,
            // and other code the has a body.

            // Instruct the lexer to calculate folding
            scintilla.SetProperty("fold", "1");
            scintilla.SetProperty("fold.compact", "1");

            // Configure a margin to display folding symbols
            scintilla.Margins[2].Type = MarginType.Symbol;
            scintilla.Margins[2].Mask = Marker.MaskFolders;
            scintilla.Margins[2].Sensitive = true;
            scintilla.Margins[2].Width = 20;

            // Set colors for all folding markers
            for (int i = 25; i <= 31; i++)
            {
                scintilla.Markers[i].SetForeColor(SystemColors.ControlLightLight);
                scintilla.Markers[i].SetBackColor(SystemColors.ControlDark);
            }

            // Configure folding markers with respective symbols
            scintilla.Markers[Marker.Folder].Symbol = MarkerSymbol.BoxPlus;
            scintilla.Markers[Marker.FolderOpen].Symbol = MarkerSymbol.BoxMinus;
            scintilla.Markers[Marker.FolderEnd].Symbol = MarkerSymbol.BoxPlusConnected;
            scintilla.Markers[Marker.FolderMidTail].Symbol = MarkerSymbol.TCorner;
            scintilla.Markers[Marker.FolderOpenMid].Symbol = MarkerSymbol.BoxMinusConnected;
            scintilla.Markers[Marker.FolderSub].Symbol = MarkerSymbol.VLine;
            scintilla.Markers[Marker.FolderTail].Symbol = MarkerSymbol.LCorner;

            // Enable automatic folding
            scintilla.AutomaticFold = (AutomaticFold.Show | AutomaticFold.Click | AutomaticFold.Change);

        }

        #region Brace Index
        private static bool IsBrace(int c)
        {
            switch (c)
            {
                case '(':
                case ')':
                case '[':
                case ']':
                case '{':
                case '}':
                case '<':
                case '>':
                    return true;
            }

            return false;
        }
        #endregion





        #region Brace Matching

        

        private void scintilla_UpdateUI(object sender, UpdateUIEventArgs e)
        {
            // Has the caret changed position?
            var caretPos = scintilla.CurrentPosition;
            if (lastCaretPos != caretPos)
            {
                lastCaretPos = caretPos;
                var bracePos1 = -1;
                var bracePos2 = -1;

                // Is there a brace to the left or right?
                if (caretPos > 0 && IsBrace(scintilla.GetCharAt(caretPos - 1)))
                    bracePos1 = (caretPos - 1);
                else if (IsBrace(scintilla.GetCharAt(caretPos)))
                    bracePos1 = caretPos;

                if (bracePos1 >= 0)
                {
                    // Find the matching brace
                    bracePos2 = scintilla.BraceMatch(bracePos1);
                    if (bracePos2 == Scintilla.InvalidPosition)
                    {
                        scintilla.BraceBadLight(bracePos1);
                        scintilla.HighlightGuide = 0;
                    }
                    else
                    {
                        scintilla.BraceHighlight(bracePos1, bracePos2);
                        scintilla.HighlightGuide = scintilla.GetColumn(bracePos1);
                    }
                }
                else
                {
                    // Turn off brace matching
                    scintilla.BraceHighlight(Scintilla.InvalidPosition, Scintilla.InvalidPosition);
                    scintilla.HighlightGuide = 0;
                }
            }
        }
        #endregion


        #region Line Numbering
        private int maxLineNumberCharLength;
        private void scintilla_TextChanged(object sender, EventArgs e)
        {
            // Did the number of characters in the line number display change?
            // i.e. nnn VS nn, or nnnn VS nn, etc...
            var maxLineNumberCharLength = scintilla.Lines.Count.ToString().Length;
            if (maxLineNumberCharLength == this.maxLineNumberCharLength)
                return;

            // Calculate the width required to display the last line number
            // and include some padding for good measure.
            const int padding = 2;
            scintilla.Margins[0].Width = scintilla.TextWidth(Style.LineNumber, new string('9', maxLineNumberCharLength + 1)) + padding;
            this.maxLineNumberCharLength = maxLineNumberCharLength;
        }
        #endregion


        // Zoom in, out controls.
        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            scintilla.ZoomIn();
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            scintilla.ZoomOut();
        }

        // Auto complete menu
        private void scintilla_CharAdded(object sender, CharAddedEventArgs e)
        {
            // Find the word start
            var currentPos = scintilla.CurrentPosition;
            var wordStartPos = scintilla.WordStartPosition(currentPos, true);

            // Display the autocompletion list
            var lenEntered = currentPos - wordStartPos;
            if (lenEntered > 0)
            {
                if (!scintilla.AutoCActive)
                    // Possible keywords
                    // (When the lexer is available, a more complex autocomplete menu will be possible.
                    scintilla.AutoCShow(lenEntered, "Link Game Screen if import for while true false else null this ffc item global switch case default break continue bool byte char const decimal double enum float int long short static string struct void script");
            }
        }


        // Used for searching for a
        // piece of code.

        private void HighlightWord(string text)
        {
            // Indicators 0-7 could be in use by a lexer
            // so we'll use indicator 8 to highlight words.
            const int NUM = 8;

            // Remove all uses of our indicator
            scintilla.IndicatorCurrent = NUM;
            scintilla.IndicatorClearRange(0, scintilla.TextLength);

            // Update indicator appearance
            scintilla.Indicators[NUM].Style = IndicatorStyle.StraightBox;
            scintilla.Indicators[NUM].Under = true;
            scintilla.Indicators[NUM].ForeColor = Color.Green;
            scintilla.Indicators[NUM].OutlineAlpha = 50;
            scintilla.Indicators[NUM].Alpha = 30;

            // Search the document
            scintilla.TargetStart = 0;
            scintilla.TargetEnd = scintilla.TextLength;
            scintilla.SearchFlags = SearchFlags.None;
            while (scintilla.SearchInTarget(text) != -1)
            {
                // Mark the search results with the current indicator
                scintilla.IndicatorFillRange(scintilla.TargetStart, scintilla.TargetEnd - scintilla.TargetStart);

                // Search the remainder of the document
                scintilla.TargetStart = scintilla.TargetEnd;
                scintilla.TargetEnd = scintilla.TextLength;
            }
        }

        // Find and highlight all usages of the search
        // query in the current script file
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            string query = toolStripTextBox1.Text;
            HighlightWord(query);

        }

        // This will be removed.
        private void toolStripTextBox1_Click(object sender, EventArgs e)
        {

        }

        // Create a new script file. (Calls LoadNew which then
        // calls the NewScript file.
        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            LoadNew();
        }

        // Undo
        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            if (scintilla.CanUndo)
            {
                scintilla.Undo();
            }
        }

        // Redo
        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (scintilla.CanRedo)
            {
                scintilla.Redo();
            }
            
        }

        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadNew();
        }



        #region File Creation
        public void LoadNew()
        {
            ns = new NewScript();
            ns.ShowDialog();

            switch (ns.ScriptType)
            {
                case "item": // If the user makes an item script.
                    scintilla.Text =
                        "import \"std.zh\" \n\nitem script " + ns.ScriptName + " {\n\n\tvoid run(){\n\n\t}\n\n}";
                    treeView1.Nodes[0].Nodes.Add(null, ns.ScriptName + ".z");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\" + ns.ScriptName + ".z", scintilla.Text);
                    treeView1.ExpandAll();

                    break;

                case "FFC": // If the user make a FFC script.
                    scintilla.Text =
                        "import \"std.zh\" \n\nffc script " + ns.ScriptName + " {\n\n\tvoid run(){\n\n\t}\n\n}";
                    treeView1.Nodes[0].Nodes.Add(null, ns.ScriptName + ".z");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\" + ns.ScriptName + ".z", scintilla.Text);
                    treeView1.ExpandAll();

                    break;

                case "Global": // If the user makes a global script.
                    scintilla.Text =
                        "import \"std.zh\" \n\nglobal script " + ns.ScriptName + " {\n\n\tvoid run(){\n\n\t}\n\n}";
                    treeView1.Nodes[0].Nodes.Add(null, ns.ScriptName + ".z");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\" + ns.ScriptName + ".z", scintilla.Text);
                    treeView1.ExpandAll();

                    break;

                default: // If the user makes a header.
                    scintilla.Text = "";
                    treeView1.Nodes[0].Nodes.Add(null, ns.ScriptName + ".zh");
                    File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\" + ns.ScriptName + ".zh", scintilla.Text);
                    treeView1.ExpandAll();

                    break;
                    
            }
        }
        #endregion





        // Show/Open the file that was clicked in the
        // heirarchy.
        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            string nodeName = e.Node.Text;

            if (e.Node.Text == "Scripts")
            {

            }
            else
            {
                if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\" + nodeName))
                {

                    string contents = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\" + nodeName);
                    scintilla.Text = contents;
                } else
                {
                    string contents1 = File.ReadAllText(nodeName);
                    scintilla.Text = contents1;
                }
            }
        }

        #region Power
        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
        #endregion

        // Select all
        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.SelectAll();
        }

        //Close the currently selected script file
        private void closeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode.Remove();
            
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }

        public void OpenFile()
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog(); // New file chooser
                                                           // OFD Properties
                ofd.Filter = "ZScript Script Files (*.z)|*.z|ZScript Header Files (*.zh)|*.zh"; // Keep the file chooser from loading anything other than .z and.zh files.


                ofd.ShowDialog();

                // Process the file.
                string filepath = File.ReadAllText(ofd.FileName); // Read the contents.
                scintilla.Text = filepath; // Display in the editor.
                treeView1.Nodes[0].Nodes.Add(null, ofd.FileName); // Add a node for it in the tree view.
            } catch (IOException ioe)
            {
                Console.WriteLine(ioe.StackTrace); // Print stacktrace in debug console. (This will only work in the IDE or cmd ofcourse.)
            }
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            OpenFile();
        }


        // Undo, Redo
        private void undoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Can you Undo?
            if (scintilla.CanUndo)
            {
                scintilla.Undo();
            }

        }

        private void redoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Can you Redo?
            if (scintilla.CanRedo)
            {
                scintilla.Redo();
            }
        }

        // Insert a comment block
        // where the caret is.
        private void commentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.InsertText(scintilla.CurrentPosition, "/*  */");
        }


        /*
         * Clipboard Functions
         * 
         */


        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Cut();
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Copy();
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.Paste();
        }

        // Add an indentation
        // where the caret is.
        private void indentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scintilla.InsertText(scintilla.CurrentPosition, "\t");
        }



        // Help Menu

        // ZC Website
        private void zeldaClassicToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://zeldaclassic.com"); // ZC Website
        }

        private void zSExpressToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.purezc.net/forums/index.php?showtopic=72780#entry1026329"); // ZS-E
        }

        // Scripting Basics
        private void scriptingBasicsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.purezc.net/forums/index.php?showtopic=65586"); // Basic
        }

        private void scriptingIntermediateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.purezc.net/forums/index.php?showtopic=65588"); // Med
        }

        private void scriptingAdvancedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://www.purezc.net/forums/index.php?showtopic=65589"); // Advanced
        }

        // About ZS-E
        // No one usually clicks on about, but it's there.
        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 ab1 = new AboutBox1();
            ab1.ShowDialog();
        }

        // Save
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            saveScript(); // Call saveScript.
        }


        public void saveScript()
        {
            // I SHOULD and probably will add a try/catch statement in the future. 
            string node = treeView1.SelectedNode.Text;

            // If the user tries to save the code to the script folder, stop them. If they
            // select an average file, then... well... save.
            if(node == "Scripts")
            {
                MessageBox.Show("You can't save the script contents directly to your script folder, silly!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            } else
            {
                File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "\\Scripts\\" + node, scintilla.Text);
            }
        }

        private void showConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Parser.ParserConsole parsec = new Parser.ParserConsole();
            if (parsec.Visible == false)
            {
                parsec.Show();
            }
        }

        private void hideConsoleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Parser.ParserConsole parsec = new Parser.ParserConsole();
            if (parsec.Visible == true)
            {
                parsec.Hide();
            }
        }

        private void addinsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Add-in functionality will be added once more important features are
            // added.
        }
    }
}
