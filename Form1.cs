using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Compression;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using selectFiles;
using minClock;
using secClock;

public struct Marble
{
	public int x;
	public int y;
    public int id;
    public bool alive;
}

public struct Hole
{
	public int x;
	public int y;
    public int id;
};

public struct Wall
{
	public int x1;
	public int y1;
    public int x2;
    public int y2;
};

namespace ass2adampoland
{
	

	public partial class Form1 : Form
	{
		private Image puzzle;
        private Image pausePic;
		private picDesc[,] twoDArray;
        Wall[] walls;
        Marble[] marbles;
		Hole[] holes;
		int numberOfMarbles;
        bool lost=false;

        string tempDelete;
        string filePath;

        private bool running;
        DateTime startTime;
        TimeSpan currentTimePassed, pauseTime;
        string player;

        int size;
		int sizew;
		int sizeh;

		//double ratio;
		double buttonWidth;
		double buttonHeight;
        private int oldWidth;
        private int oldHeight;
        private TableLayoutPanel panelThingUno;
        private TableLayoutPanel panelThingDos;
        private bool paused = false;

        private string selectedDir;

        private List<ScoreItem> ExistingScoresList;



        public Form1()
		{
			InitializeComponent();
            fileSelector chooseFile = new fileSelector();
           if(chooseFile.ShowDialog()==DialogResult.OK){
                puzzle = Image.FromFile(chooseFile.puzzlePic);
                
                filePath = chooseFile.tempDirectory;
                selectedDir = chooseFile.dirName;
                sizew = puzzle.Width / 7;
                sizeh = puzzle.Height / 7;
                tempDelete = chooseFile.tempDelete;

                string scoresFile = "scores.bin";

                //Path.Combine(tempDelete, "scores.bin");

                FileInfo PersonFileInfo = new FileInfo(scoresFile);

                using (ZipArchive zip = ZipFile.Open(selectedDir, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry entry in zip.Entries)
                    {
                        if (entry.Name == scoresFile)
                        {
                           
                            if (PersonFileInfo.Exists == false)
                            {
                                ExistingScoresList = new List<ScoreItem>();
                            }
                            else
                            {
                                File.Delete("scores.bin");

                                entry.ExtractToFile(scoresFile);

                                using (FileStream stream = new FileStream(scoresFile, FileMode.Open))
                                {
                                    IFormatter formatter = new BinaryFormatter();

                                    ExistingScoresList = (List<ScoreItem>)formatter.Deserialize(stream);

                                    Console.WriteLine("cool");

                                    Console.WriteLine(ExistingScoresList.ToString());
                                    foreach (ScoreItem curScore in ExistingScoresList)
                                    {
                                        addToList(curScore);
                                    }
                                }
                            }
                        }
                        //else
                        //{
                        //    MessageBox.Show("NOT FOUND~!!!!");

                        //}
                    }
                }

                //using (ZipArchive archive = ZipFile.Open(selectedDir, ZipArchiveMode.Update))
                //{
                //    ///////////////////////////////////////////////////////////////////////
                //    if (PersonFileInfo.Exists == false)
                //    {
                //        ExistingScoresList = new List<ScoreItem>();
                //    }
                //    else
                //    {
                //        using (FileStream stream = new FileStream(scoresFile, FileMode.Open))
                //        {
                //            IFormatter formatter = new BinaryFormatter();

                //            ExistingScoresList = (List<ScoreItem>)formatter.Deserialize(stream);

                //            Console.WriteLine("cool");

                //            Console.WriteLine(ExistingScoresList.ToString());
                //            foreach (ScoreItem curScore in ExistingScoresList)
                //            {
                //                addToList(curScore);
                //            }
                //        }
                //    }
                //    //////////////////////////////////////////////////////////////////////////////
                   

                //}


               

                start();
           }
           else
           {
                Console.WriteLine("something went wrong");
                //this.Close();
           }
            oldWidth = this.Width;
            oldHeight = this.Height;
            //puzzle = Image.FromFile("puzzle.jpg");
            pausePic = Image.FromFile("pause.png");
            //puzzle = Image.FromFile(puzzlePic);


        }

     
		private void start()
		{
            player = Microsoft.VisualBasic.Interaction.InputBox("Please Enter your name", "Score Manager", "N/A", -1, -1);

            //MessageBox.Show(filePath);
            string[] linesFromFile = System.IO.File.ReadAllLines(filePath);
			string[] pieces = linesFromFile[0].Split(' ');
			Int32.TryParse(pieces[1], out numberOfMarbles);
			marbles = new Marble[numberOfMarbles];
			holes = new Hole[numberOfMarbles];
            walls = new Wall[20];
            string[] wallsData;

			for (int i = 1; i <= numberOfMarbles; i++)
			{
				string[] marblePos = linesFromFile[i].Split(' ');
				Int32.TryParse(marblePos[0], out marbles[i-1].x);
				Int32.TryParse(marblePos[1], out marbles[i-1].y);
                marbles[i-1].alive = true;
                marbles[i-1].id = i;

            }

            int holeCount = 0;
			for (int i = numberOfMarbles+1; i <= (numberOfMarbles*2); i++)
			{
				string[] marblePos = linesFromFile[i].Split(' ');
				Int32.TryParse(marblePos[0], out holes[holeCount].x);
				Int32.TryParse(marblePos[1], out holes[holeCount].y);
                holes[holeCount].id=holeCount+1;
				holeCount++;
			}

            //wallsData = linesFromFile.Last().Split(' ');
            //wallsData = linesFromFile[j].Split(' ');
			int wallCount = 0;
            for (int j = (numberOfMarbles * 2) + 1; j < linesFromFile.Count(); j++)
            {
                wallsData = linesFromFile[j].Split(' ');

                for (int i = 0; i < linesFromFile.Count() - numberOfMarbles * 2; i = i + 1)
                {
                    Int32.TryParse(wallsData[0], out walls[i].x1);
                    Int32.TryParse(wallsData[1], out walls[i].y1);
                    Int32.TryParse(wallsData[2], out walls[i].x2);
                    Int32.TryParse(wallsData[3], out walls[i].y2);

                    wallCount++;
                }
            }
            Int32.TryParse(pieces[0], out size);

			//if (sizew > sizeh)
			//{
			//	ratio = sizew / (double)sizeh;
			//	buttonWidth = 400 / size;
			//	buttonHeight = buttonWidth / ratio;
			//}
			//else
			//{
			//	ratio = sizeh / (double)sizew;
			//	buttonHeight = 400 / size;
			//	buttonWidth = buttonHeight / ratio;
			//}

			twoDArray = new picDesc[size, size];

            panelThingUno = new TableLayoutPanel();
            panelThingDos = new TableLayoutPanel();
            float ratio = sizeh / sizew * 100;
            if (sizew > sizeh)
            {
                
                panelThingUno.ColumnCount = 1;
                panelThingUno.RowCount = 2;
                panelThingUno.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
                panelThingUno.RowStyles.Add(new RowStyle(SizeType.Percent, ratio));
                panelThingUno.RowStyles.Add(new RowStyle(SizeType.Percent, 100 - ratio));
                panelThingUno.Dock = DockStyle.Fill;
                panelThingUno.Margin = new Padding(0);
                tablething.Controls.Add(panelThingUno, 0, 0);
            }
            else
            {
                panelThingUno.ColumnCount = 2;
                panelThingUno.RowCount = 1;
                panelThingUno.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, ratio));
                panelThingUno.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100-ratio));
                panelThingUno.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                panelThingUno.Dock = DockStyle.Fill;
                panelThingUno.Margin = new Padding(0);
                tablething.Controls.Add(panelThingUno, 0, 0);
            }


            panelThingDos.ColumnCount = size;
            panelThingDos.RowCount = size;
            for(int i = 0; i <size; i++)
            {
                panelThingDos.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100 / (float)size));
                panelThingDos.RowStyles.Add(new RowStyle(SizeType.Percent, 100 / (float)size));

            }
            panelThingDos.Dock = DockStyle.Fill;
            panelThingDos.Margin = new Padding(0);
            panelThingUno.Controls.Add(panelThingDos, 0, 0);
            buttonWidth = sizew;
            buttonHeight = sizeh;


            update();
            setUp();
            clok1.Start();
            minClok1.Start();
			
		}

        private void setUp()
        {
            
                    if (walls[0].x1 != walls[0].x2)
                    {
                        if (walls[0].x1 > walls[0].x2)
                        {
                            //bottom then top
                            twoDArray[walls[0].x1, walls[0].y1].wallUp = true;
                            twoDArray[walls[0].x2, walls[0].y2].wallDown = true;
                        }
                        else
                        {
                            //down then up
                            twoDArray[walls[0].x1, walls[0].y1].wallDown = true;
                            twoDArray[walls[0].x2, walls[0].y2].wallUp = true;
                        }
                    }
                    else
                    {
                        //left right
                        if (walls[0].y1 != walls[0].y2)
                        {
                            twoDArray[walls[0].x1, walls[0].y1].wallLeft = true;
                            twoDArray[walls[0].x2, walls[0].y2].wallRight = true;
                        }
                        else
                        {
                            //right then left
                            twoDArray[walls[0].x1, walls[0].y1].wallRight = true;
                            twoDArray[walls[0].x2, walls[0].y2].wallLeft = true;
                        }
                    }
           
            update();
        }
        private bool didIt = false;
		private void update()
		{

			for (int r = 0, roffset = 0; r < size; r++)
			{
                for (int c = 0, coffset = 0; c < size; c++)
                {
                    if (didIt == false || twoDArray == null)
                    {
                        this.twoDArray[r, c] = new picDesc();
                        panelThingDos.Controls.Add(twoDArray[r, c], c, r);

                    }
                    this.twoDArray[r, c].Location = new System.Drawing.Point(12 + coffset, 12 + roffset);
					this.twoDArray[r, c].Name = "picr" + c.ToString() + "c" + r.ToString();
					this.twoDArray[r, c].Size = new System.Drawing.Size((int)buttonWidth, (int)buttonHeight);
                    this.twoDArray[r, c].BorderStyle = BorderStyle.FixedSingle;
                    twoDArray[r, c].Dock = DockStyle.Fill;
                    twoDArray[r, c].Margin = new Padding(0);
                    twoDArray[r, c].SizeMode = PictureBoxSizeMode.StretchImage;
                    //twoDArray[r, c].Click += new EventHandler(Pic_size)
                    //this.Controls.Add(this.twoDArray[r, c]);
					coffset += (int)buttonWidth;



                    //there is probably a better way to do this
                    //this.twoDArray[r, c].BringToFront();

					//picturebox should already have correct ratio here
					int displayWidth = (int)buttonWidth;
					int displayHeight = (int)buttonHeight;

					Pen bp = new Pen(Color.Black);
					Brush bf = new SolidBrush(Color.White);
					Font f = new Font("Arial", 24.0f);
					StringFormat sf = new StringFormat();
					sf.LineAlignment = StringAlignment.Center;
					sf.Alignment = StringAlignment.Center;

					//Easy solution. Make the picturebox the right size.
					Bitmap bm = new Bitmap(displayWidth, displayHeight);
					Graphics g = Graphics.FromImage(bm);
					Rectangle rect = new Rectangle(0, 0, displayWidth, displayHeight);


                    if (paused == true)
                    {
                        g.DrawImage(pausePic, rect, puzzle.Width, puzzle.Height, puzzle.Width, puzzle.Height, GraphicsUnit.Pixel);


                    }
                    else if (r == 0 && c == 0) //topleft
                    {
                        for (int i = 0; i < numberOfMarbles; i++)
                        {

                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {
                                g.DrawImage(puzzle, rect, 2 * (puzzle.Width / 7), 5 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 0 * (puzzle.Width / 7), 3 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);

                                break;
                            }
                            else
                            {
                                g.DrawImage(puzzle, rect, 5 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                            }
                        }
                    }
                    //top middle
                    else if (r == 0 && c < size - 1)
                    {
                        for (int i = 0; i < numberOfMarbles; i++)
                        {
                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {

                                g.DrawImage(puzzle, rect, 0 * (puzzle.Width / 7), 5 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);

                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 5 * (puzzle.Width / 7), 2 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else
                                g.DrawImage(puzzle, rect, 3 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                        }
                    }
                    //bottom left
                    else if (r == size - 1 && c == 0)
                    {

                        for (int i = 0; i < numberOfMarbles; i++)
                        {
                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {
                                g.DrawImage(puzzle, rect, 4 * (puzzle.Width / 7), 5 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 2 * (puzzle.Width / 7), 3 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else
                                g.DrawImage(puzzle, rect, 0 * (puzzle.Width / 7), 1 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                        }
                    }
                    //bottom middle
                    else if (r == size - 1 && c < size - 1)
                    {
                        for (int i = 0; i < numberOfMarbles; i++)
                        {
                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {
                                g.DrawImage(puzzle, rect, 1 * (puzzle.Width / 7), 5 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 6 * (puzzle.Width / 7), 2 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else
                                g.DrawImage(puzzle, rect, 4 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                        }
                    }
                    //top right
                    else if (r == 0 && c == size - 1)
                    {
                        for (int i = 0; i < numberOfMarbles; i++)
                        {
                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {
                                g.DrawImage(puzzle, rect, 5 * (puzzle.Width / 7), 5 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 3 * (puzzle.Width / 7), 3 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else
                                g.DrawImage(puzzle, rect, 1 * (puzzle.Width / 7), 1 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                        }
                    }
                    //left side
                    else if (r < size - 1 && c == 0)
                    {
                        for (int i = 0; i < numberOfMarbles; i++)
                        {
                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {
                                g.DrawImage(puzzle, rect, 5 * (puzzle.Width / 7), 4 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 3 * (puzzle.Width / 7), 2 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else
                                g.DrawImage(puzzle, rect, 1 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                        }
                    }
                    //right side
                    else if (r < size - 1 && c == size - 1)
                    {
                        for (int i = 0; i < numberOfMarbles; i++)
                        {
                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {
                                g.DrawImage(puzzle, rect, 6 * (puzzle.Width / 7), 4 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 4 * (puzzle.Width / 7), 2 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else
                                g.DrawImage(puzzle, rect, 2 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                        }
                    }
                    //bottom right
                    else if (r == size - 1 && c == size - 1)
                    {
                        for (int i = 0; i < numberOfMarbles; i++)
                        {
                            if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                            {
                                g.DrawImage(puzzle, rect, 6 * (puzzle.Width / 7), 5 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else if (holes[i].x == r && holes[i].y == c)
                            {
                                g.DrawImage(puzzle, rect, 4 * (puzzle.Width / 7), 3 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                break;
                            }
                            else
                                g.DrawImage(puzzle, rect, 2 * (puzzle.Width / 7), 1 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                        }
                    }
                    //middle boxes
                    else
                    {
                        if (r == 1 && c == 1)
                        {
                            for (int i = 0; i < numberOfMarbles; i++)
                            {
                                if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                                {
                                    g.DrawImage(puzzle, rect, 6 * (puzzle.Width / 7), 4 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                    g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                    break;
                                }
                                else if (holes[i].x == r && holes[i].y == c)
                                {
                                    g.DrawImage(puzzle, rect, 4 * (puzzle.Width / 7), 3 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                    g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                    break;
                                }
                                else
                                {
                                    g.DrawImage(puzzle, rect, 0 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                    
                                }
                            }
                        }//cheating
                        else if (r == 1 && c == 2)
                        {
                            for (int i = 0; i < numberOfMarbles; i++)
                            {
                                if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                                {
                                    g.DrawImage(puzzle, rect, 5 * (puzzle.Width / 7), 4 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                    g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                    break;
                                }
                                else if (holes[i].x == r && holes[i].y == c)
                                {
                                    g.DrawImage(puzzle, rect, 3 * (puzzle.Width / 7), 2 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                    g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                    break;
                                }
                                else
                                {
                                    g.DrawImage(puzzle, rect, 0 * (puzzle.Width / 7), 1 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                    
                                }
                            }
                        }
                        else
                        {

                            for (int i = 0; i < numberOfMarbles; i++)
                            {
                                if (marbles[i].x == r && marbles[i].y == c && marbles[i].alive == true)
                                {
                                    g.DrawImage(puzzle, rect, 4 * (puzzle.Width / 7), 4 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                    g.DrawString(marbles[i].id.ToString(), f, bf, rect, sf);
                                    break;
                                }
                                else if (holes[i].x == r && holes[i].y == c)
                                {
                                    g.DrawImage(puzzle, rect, 2 * (puzzle.Width / 7), 2 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                    g.DrawString(holes[i].id.ToString(), f, bf, rect, sf);
                                    break;
                                }
                                else
                                {
                                    //if (twoDArray[r, c].wallRight == true)
                                    //{
                                    //    g.DrawImage(puzzle, rect, 1 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                    //    break;
                                    //}
                                    //if (twoDArray[r, c].wallLeft == true)
                                    //{
                                    //    g.DrawImage(puzzle, rect, 2 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);
                                    //    break;
                                    //}
                                    //else
                                    //{
                                    g.DrawImage(puzzle, rect, 0 * (puzzle.Width / 7), 0 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                                    //}

                                }
                            }
                        }
                    }

                    if (lost == true)
                        g.DrawImage(puzzle, rect, 6 * (puzzle.Width / 7), 6 * (puzzle.Height / 7), puzzle.Width / 7, puzzle.Height / 7, GraphicsUnit.Pixel);

                    this.twoDArray[r, c].Image = bm;
				}
				roffset += (int)buttonHeight;
			}
            

            didIt = true;
        }

        private void lose(int r, int c)
        {
            output.Text = "You Lost!!!!!!!!!";
            clok1.Stop();
            minClok1.Stop();
            lost = true;

        }

        private void winCheck()
        {
            int check = 0;
            //if all marbles are in the write holes you win
            for (int i = 0; i < numberOfMarbles; i++)
            {
                if (marbles[i].alive==false)
                {
                    check++;
                }
                
                
            }
            if (check==numberOfMarbles)
            {

                win();
            }
        }

        private void win()
        {
            minClok1.Pause();
            clok1.Pause();
            Console.WriteLine(minClok1.Minutes.ToString() + ":" + clok1.Seconds.ToString());
            ListViewItem lvi = new ListViewItem();
            lvi.Text = player; //(This is the 'name' property of our form)
            lvi.SubItems.Add(minClok1.Minutes.ToString() + ":" + clok1.Seconds.ToString());
            
            lvi.SubItems.Add(rollCount.Text);
            lvScores.Items.Add(lvi);
            ScoreItem s = new ScoreItem();
            s.Name = player;
            s.Time = minClok1.Minutes.ToString() + ":" + clok1.Seconds.ToString();
            s.Moves = Convert.ToInt32(rollCount.Text);
            ExistingScoresList.Add(s);
            save();

            output.Text = "You Win!!!";
        }

        private void addToList(ScoreItem score)
        {
            ListViewItem lvi = new ListViewItem();
            lvi.Text = score.Name;
            lvi.SubItems.Add(minClok1.Minutes.ToString() + clok1.Seconds.ToString());
            lvi.SubItems.Add(score.Moves.ToString());
            
            lvScores.Items.Add(lvi);
        }

        private bool match()
        {
            for (int i = 0; i < numberOfMarbles; i++)
            {
                if (marbles[i].x > 100)
                {
                    win();
                    return true;
                }

            }
            return false;
        }

        private string isSomethingThere(int x, int y, int id, string dir)
		{
			for (int i = 0; i < numberOfMarbles; i++)
			{
				if (marbles[i].x == x && marbles[i].y == y && marbles[i].alive == true)
				{
					Console.WriteLine("marble");
					return "marble";
				}
				else if (holes[i].x == x && holes[i].y == y)
				{
                    Console.WriteLine(holes[i].id + "vs"+ id);
                    if (holes[i].id != id)
                    {
                        lose(holes[i].x, holes[i].y);
                    }
					return "hole";
				}else if ((walls[i].x1 == x && walls[i].y1 == y&& dir == "right")||(walls[i].x2 == x && walls[i].y2 == y && dir == "left"))
                {
                    Console.WriteLine("Wall");
                    return "wall";
                }
			}
			return "nope";
		}


        private void count()
        {
            int count = (Convert.ToInt32(rollCount.Text) + 1);
            rollCount.Text = count.ToString();

        }

        private void btnup_Click(object sender, EventArgs e)
		{
            count();
            bool go = true;
            Console.WriteLine("////////////////////");

            Console.WriteLine(marbles[0].x + " " + marbles[0].y);
            Console.WriteLine(marbles[1].x + " " + marbles[1].y);
            Console.WriteLine(marbles[2].x + " " + marbles[2].y);
            Console.WriteLine("beforewrwe");
            for (int column = size-1; column >= 0; column--)
            {
                for (int row = 0; row < size; row++)
                {

                    for (int i = 0; i < numberOfMarbles; i++)
                    {
                        go = true;
                        //Console.WriteLine(size);

                        if (marbles[i].x == row && marbles[i].y == column && marbles[i].alive == true)//gets the marble if there is one
                        {

                            while (marbles[i].x > 0 && go)//moves it
                            {
                                if (isSomethingThere(marbles[i].x - 1, marbles[i].y, marbles[i].id, "up") == "nope")
                                {
                                    marbles[i].x -= 1;
                                }
                                else
                                {
                                    if (isSomethingThere(marbles[i].x - 1, marbles[i].y, marbles[i].id, "up") == "marble")
                                    {
                                        Console.WriteLine("marble");
                                        go = false;
                                    }
                                    else if (isSomethingThere(marbles[i].x- 1, marbles[i].y, marbles[i].id,"up") == "wall")
                                    {
                                        marbles[i].x -= 1;
                                        go = false;
                                    }

                                    else
                                    {
                                        //hole
                                        marbles[i].alive = false;
                                        go = false;
                                    }
                                }

                            }


                        }

                    }
                    //nothing
                }
            }
            Console.WriteLine(marbles[0].x + " " + marbles[0].y);
            Console.WriteLine(marbles[1].x + " " + marbles[1].y);
            Console.WriteLine(marbles[2].x + " " + marbles[2].y);
          
                winCheck();
                update();
            
        }

		private void btndown_Click(object sender, EventArgs e)
		{
            count();
			bool go = true;
			Console.WriteLine(marbles[0].x + " " + marbles[0].y);
			Console.WriteLine(marbles[1].x + " " + marbles[1].y);
			Console.WriteLine(marbles[2].x + " " + marbles[2].y);
			Console.WriteLine("beforewrwe");
            for (int row = 0; row < size; row++)
            {
                for (int column = size-1; column >= 0; column--)
                {
                    for (int i = 0; i < numberOfMarbles; i++)
                    {
                        go = true;

                        if (marbles[i].x == row && marbles[i].y == column && marbles[i].alive == true)
                        {
                            Console.WriteLine("marvle found down" + " "+ marbles[i].id);

                            while (marbles[i].x < size-1 && go)
                            {
                                if (isSomethingThere(marbles[i].x +1, marbles[i].y, marbles[i].id, "down") == "nope")
                                {
                                    marbles[i].x += 1;
                                }
                                else
                                {
                                    if (isSomethingThere(marbles[i].x + 1, marbles[i].y, marbles[i].id, "down") == "marble")
                                    {
                                        Console.WriteLine("marble");
                                        go = false;
                                    }
                                    else
                                    {
                                        //hole
                                        marbles[i].alive = false;
                                        go = false;
                                    }
                                }

                            }


                        }

                    }
                    //nothing
                }
            }
            Console.WriteLine(marbles[0].x + " " + marbles[0].y);
			Console.WriteLine(marbles[1].x + " " + marbles[1].y);
			Console.WriteLine(marbles[2].x + " " + marbles[2].y);
            
                winCheck();
                update();
            
        }

		private void btnright_Click(object sender, EventArgs e)
		{
            count();
			bool go = true;
			Console.WriteLine("////////////////////");

			Console.WriteLine(marbles[0].x + " " + marbles[0].y);
			Console.WriteLine(marbles[1].x + " " + marbles[1].y);
			Console.WriteLine(marbles[2].x + " " + marbles[2].y);
			Console.WriteLine("beforewrwe");
            for (int column = size-1; column >= 0; column--)
            {
                for (int row = size-1; row >= 0; row--)
                    {
               
                    for (int i = 0; i < numberOfMarbles; i++)
                    {
                        go = true;
                        Console.WriteLine(size);

                        if (marbles[i].x == row && marbles[i].y == column && marbles[i].alive == true)//gets the marble if there is one
                        {

                            while (marbles[i].y < size-1 && go)//moves it
                            {
                                if (isSomethingThere(marbles[i].x, marbles[i].y+1, marbles[i].id,"right") == "nope")
                                {
                                    marbles[i].y+= 1;
                                }
                                else
                                {
                                    if (isSomethingThere(marbles[i].x, marbles[i].y+1, marbles[i].id,"right") == "marble")
                                    {
                                        Console.WriteLine("marble");
                                        go = false;
                                    }
                                    else if (isSomethingThere(marbles[i].x, marbles[i].y, marbles[i].id,"right") == "wall")
                                    {
                                        //marbles[i].y += 1;
                                        go = false;
                                        break;
                                    }
                                    else if (isSomethingThere(marbles[i].x, marbles[i].y+1, marbles[i].id,"right") == "wall")
                                    {
                                        marbles[i].y += 1;
                                        go = false;
                                    }
                                   

                                    else
                                    {
                                    
                                        marbles[i].alive = false;
                                        go = false;
                                    }
                                }

                            }


                        }

                    }
                    //nothing
                }
            }
            Console.WriteLine(marbles[0].x + " " + marbles[0].y);
			Console.WriteLine(marbles[1].x + " " + marbles[1].y);
			Console.WriteLine(marbles[2].x + " " + marbles[2].y);
           
                winCheck();
                update();
            
        }

        

		private void btnleft_Click(object sender, EventArgs e)
		{
            count();
			bool go = true;
			Console.WriteLine("////////////////////");

			Console.WriteLine(marbles[0].x + " " + marbles[0].y);
			Console.WriteLine(marbles[1].x + " " + marbles[1].y);
			Console.WriteLine(marbles[2].x + " " + marbles[2].y);
			Console.WriteLine("beforewrwe");
            for (int column = 0; column < size; column++)
            {
                for (int row = 0; row < size; row++)
                {

                    for (int i = 0; i < numberOfMarbles; i++)
                    {
                        go = true;
                        Console.WriteLine(size);

                        if (marbles[i].x == row && marbles[i].y == column && marbles[i].alive == true)//gets the marble if there is one
                        {

                            while (marbles[i].y > 0 && go)//moves it
                            {
                                if (isSomethingThere(marbles[i].x, marbles[i].y - 1, marbles[i].id,"left") == "nope")
                                {
                                    marbles[i].y -= 1;
                                }
                                else
                                {
                                    if (isSomethingThere(marbles[i].x, marbles[i].y - 1, marbles[i].id,"left") == "marble")
                                    {
                                        Console.WriteLine("marble");
                                        go = false;
                                    }
                                    else if (isSomethingThere(marbles[i].x, marbles[i].y, marbles[i].id,"left") == "wall")
                                    {
                                        //marbles[i].y -= 1;
                                        go = false;
                                        break;
                                    }
                                    else if (isSomethingThere(marbles[i].x, marbles[i].y-1, marbles[i].id,"left") == "wall")
                                    {
                                        marbles[i].y -= 1;
                                        go = false;
                                    }
        
                                    else
                                    {
                                        marbles[i].alive = false;
                                        go = false;
                                    }
                                }

                            }
                                

                        }

                    }
                    //nothing
                }
            }
            Console.WriteLine(marbles[0].x + " " + marbles[0].y);
			Console.WriteLine(marbles[1].x + " " + marbles[1].y);
			Console.WriteLine(marbles[2].x + " " + marbles[2].y);
            
                winCheck();
                update();
            

        }

        private void cleanup()
        {
            for (int r = 0; r < size; r++)
            {
                for (int c = 0; c < size; c++)
                {
                    twoDArray[r, c].Dispose();
                    twoDArray[r, c] = null;
                }
            }
            panelThingUno.Controls.Remove(panelThingDos);
            tablething.Controls.Remove(panelThingUno);
        }

        private void RemoveDirectory()
        {
            puzzle.Dispose();
            puzzle = null;

            save();
            DirectoryInfo dirInfo = new DirectoryInfo(tempDelete);
            if (dirInfo.Exists)
            {
                FileInfo[] fileList = dirInfo.GetFiles();
                //foreach (FileInfo file in fileList)
                //{
                //    File.Delete(file.FullName);
                //}
                
                Directory.Delete(tempDelete, true);
            }
            
        }

        private void selectFile_Click(object sender, EventArgs e)
        {
            cleanup();
            RemoveDirectory();
            didIt = false;
            rollCount.Text = "0";

            fileSelector chooseFile = new fileSelector();
            if (chooseFile.ShowDialog() == DialogResult.OK)
            {
                puzzle = Image.FromFile(chooseFile.puzzlePic);
                filePath = chooseFile.tempDirectory;
                sizew = puzzle.Width / 7;
                sizeh = puzzle.Height / 7;
                if (selectedDir != chooseFile.dirName) {
                    lvScores.Items.Clear();
                }
                lost = false;
            }
            else
            {
                Console.WriteLine("something went wrotn");
            }
            output.Text = "";
           start();
        }

        private void Form1_ResizeEnd(object sender, EventArgs e)
        {
            int diffWidth = Math.Abs(oldWidth - this.Width);
            int diffHeight = Math.Abs(oldHeight - this.Height);

            Console.WriteLine(">>>>>>>>>>>>");
            Console.WriteLine(diffWidth);
            Console.WriteLine(diffHeight);
            Console.WriteLine(this.Width);
            Console.WriteLine(this.Height);
            if (diffWidth > diffHeight)
            {
                this.Height = this.Width-250;
            }
            else
            {
                this.Width = this.Height;
            }

            oldWidth = this.Width;
            oldHeight = this.Height;
        }

       

        private void save()
        {
            IFormatter formatter = new BinaryFormatter();
            string scoreFile = "scores.bin";
            string ArchiveFile = selectedDir;
            //Path.Combine(selectedDir, "scores.bin");
            //Path.Combine(tempDelete, "scores.bin");

            //using (FileStream stream = new FileStream(ArchiveFile, FileMode.Create))
            //{
            //    formatter.Serialize(stream, ExistingScoresList);
            //}

            using (ZipArchive archive = ZipFile.Open(ArchiveFile, ZipArchiveMode.Update))
            {
                

                ZipArchiveEntry oldentry = archive.GetEntry("scores.bin");
                if (oldentry != null)
                {
                    oldentry.Delete();
                }

                ZipArchiveEntry entry = archive.CreateEntry("scores.bin");
                using (Stream stream = entry.Open())
                {
                    formatter.Serialize(stream, ExistingScoresList);
                }

               

            }

        }

        private void pause_Click(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            //int curtime = now - startTime;
            if (paused == true)
            {
                clok1.Start();
                minClok1.Start();

                //clok1.timer1.Enabled = false;
                running = false;
                Console.WriteLine(clok1.Seconds.ToString());
                paused = false;
                update();
            }
            else
            {
                clok1.Pause();
                minClok1.Pause();

                paused = true;
                update();
            }
        }
    }
}


