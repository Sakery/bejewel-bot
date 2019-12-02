using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Threading;

namespace gemReader
{
    public partial class Form1 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(long dwFlags, long dx, long dy, long cButtons, long dwExtraInfo);

        private const int MOUSEEVENTF_LEFTDOWN = 0x02;
        private const int MOUSEEVENTF_LEFTUP = 0x04;
        private const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        private const int MOUSEEVENTF_RIGHTUP = 0x10;

        public static string[,] last;
        public const int fc = 6;    //freeze constant
        public const int sc = 2;    //stable constant
        //color of background *** must change when background change ***
        /*
        public int[, ,] bg = new int[8, 8, 3] {
            {{18,20,33},	{41,43,54},	{17,18,28},	{40,42,52},	{18,20,32},	{43,44,58},	{18,20,32},	{40,41,52}	},
            {{43,47,59},	{18,21,32},	{44,49,60},	{17,20,30},	{44,40,51},	{17,14,26},	{44,47,61},	{28,26,31}  },
            {{22,31,42},	{42,47,58},	{17,19,30},	{44,45,56},	{17,17,28},	{40,44,53},	{29,32,37},	{44,38,53}	},
            {{44,50,59},	{18,23,33},	{45,53,62},	{26,31,43},	{47,53,63},	{20,21,32},	{41,38,50},	{18,14,26}	},
            {{28,50,53},	{51,70,75},	{28,43,50},	{52,60,70},	{28,35,45},	{47,54,62},	{26,35,44},	{54,64,72}	},
            {{42,41,52},	{28,55,54},	{55,67,73},	{31,44,50},	{57,71,77},	{30,44,49},	{58,75,78},	{35,55,57}	},
            {{34,35,43},	{59,75,79},	{37,56,59},	{61,83,85},	{41,65,65},	{62,83,85},	{39,61,62},	{63,86,86}	},
            {{64,87,88},	{34,26,46},	{63,85,87},	{34,54,57},	{60,47,75},	{37,50,56},	{45,42,60},	{27,20,44}	}
        };
        */
        public int[, ,] bg = new int[8, 8, 3] {
            {{18,18,29},	{42,42,53},	{19,14,27},	{47,40,53},	{39,22,44},	{50,41,55},	{35,20,41},	{70,51,74},	},
            {{41,41,52},	{23,18,32},	{53,46,59},	{24,18,28},	{61,48,66},	{25,17,29},	{62,47,64},	{32,20,34}	},
            {{34,37,48},	{62,53,69},	{40,26,39},	{58,46,60},	{30,19,34},	{68,52,69},	{33,19,37},	{56,42,59}	},
            {{58,50,55},	{38,29,37},	{57,47,57},	{39,24,39},	{62,51,67},	{66,52,70},	{79,66,84},	{38,25,43}	},
            {{55,42,43},	{67,56,59},	{55,43,50},	{89,73,85},	{79,63,78},	{105,91,106},{82,65,82},{43,40,39}	},
            {{84,72,69},	{59,46,44},	{80,64,64},	{68,48,53},	{100,79,86},{82,63,72},	{50,47,44},	{19,16,15}	},
            {{58,44,39},	{69,59,55},	{69,55,52},	{81,66,66},	{75,58,60},	{54,48,45},	{24,21,19},	{46,40,37}	},
            {{67,56,51},	{87,70,59},	{61,54,52},	{37,31,30},	{67,60,60},	{30,25,22},	{73,58,47},	{70,46,30}	}
        };
        public void doClick(int x, int y)
        {
            Cursor.Position = new Point(x, y);
            mouse_event(MOUSEEVENTF_LEFTDOWN | MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
        }

        public Form1()
        {
            InitializeComponent();
            all = new List<result>();
            last = new string[8, 8];
        }

        public string classify2(int mr, int mg, int mb, int dr, int dg, int db) //m=(20,20) d=(18,18)
        {
            if (mb > mg && mg > mr && db > dg && dg > dr)
                return "b2";
            if (mg >= mb && mb > mr && dg >= db && db > dr)
                return "g2";
            if (Math.Abs(mr - mb) + Math.Abs(mb - mg) + Math.Abs(dr - dg) + Math.Abs(dg - db) < 5)
                return "w2";
            if (mr > mb && mb > mg && dr > db && db > dg)
                return "r2";
            if (mb >= mr && mr > mg && db >= dr && dr > dg)
                return "p2";
            //yellow and orange??? middle only
            if (mr == mg && mg > mb)
                return "y2";
            if (mr > mg && mg > mb)
                return "o2";

            return "el";
        }

        public static int sx=0, sy=0;
        public static Size table_size = new Size(320, 320);
        public static Bitmap    table = new Bitmap(table_size.Width, table_size.Height);
        public static Graphics    gg = Graphics.FromImage(table);

        private void button1_Click(object sender, EventArgs e)
        {
            int x, y;
            int i, j;
            int point = 0;
            int clickx=-1, clicky=-1;

            for (i = 0; i < 8; i++)
                for (j = 0; j < 8; j++)
                    last[i, j] = "el";

            Size s = Screen.PrimaryScreen.Bounds.Size;
            Bitmap background = new Bitmap(s.Width, s.Height);
            Graphics g = Graphics.FromImage(background);
            g.CopyFromScreen(0, 0, 0, 0, s);
            Bitmap image1 = new Bitmap(Bitmap.FromFile(@"playbutton.bmp"));
            
            for (x = 0; x < background.Width - image1.Width; x++)
            {
                for (y = 0; y < background.Height - image1.Height; y++)
                {
                    point = 0;
                    for (i = 0; i < image1.Width; i++)
                    {
                        for (j = 0; j < image1.Height; j++)
                        {
                            if (Color.Equals(image1.GetPixel(i, j), background.GetPixel(x + i, y + j)))
                            {
                                point++;
                                if (point == image1.Width * image1.Height)
                                {
                                    clickx = x;
                                    clicky = y;
                                    //MessageBox.Show(x.ToString() + "," + (y + image1.Height).ToString());
                                    point = -2;
                                }
                            }
                            else
                            {
                                point = -1;
                                break;
                            }
                        }
                        if (point < 0)
                            break;
                    }
                    if (point == -2)
                        break;
                    //Color newColor = Color.FromArgb(pixelColor.R, 0, 0);
                    //image1.SetPixel(x, y, newColor);
                }
                if (point == -2)
                    break;
            }
            if (point == -2)    //found playbutton (must click it and wait)
            {
                doClick(clickx, clicky + 25);
                Thread.Sleep(250);
            }            

            //g.CopyFromScreen(0, 0, 0, 0, s);
            //background.Save("bla.bmp",System.Drawing.Imaging. ImageFormat.Bmp);
            //return;
            
            //find table
            //int offsetx = int.Parse(textBox1.Text);
            //int offsety = int.Parse(textBox2.Text);
            

            Bitmap desktop = new Bitmap(s.Width, s.Height);
            g = Graphics.FromImage(desktop);
            g.CopyFromScreen(0, 0, 0, 0, s);

            // start calibrate
            image1 = new Bitmap(Bitmap.FromFile(@"calibrate.bmp"));


            // Loop through the images pixels to reset color.
            for (x = 0; x < desktop.Width - image1.Width; x++)
            {
                for (y = 0; y < desktop.Height - image1.Height; y++)
                {
                    point = 0;
                    for (i = 0; i < image1.Width; i++)
                    {
                        for (j = 0; j < image1.Height; j++)
                        {
                            //Color pixelColor = image1.GetPixel(i, j);
                            //Color desktopColor = desktop.GetPixel(x + i, y + j);
                            if (Color.Equals(image1.GetPixel(i, j), desktop.GetPixel(x + i, y + j)))
                            {
                                point++;
                                if (point == image1.Width * image1.Height)
                                {
                                    sx = x;
                                    sy = y + image1.Height;
                                    //MessageBox.Show(x.ToString() + "," + (y + image1.Height).ToString());
                                    point = -2;
                                }
                            }
                            else
                            {
                                point = -1;
                                break;
                            }
                        }
                        if (point < 0)
                            break;
                    }
                    if (point == -2)
                        break;
                    //Color newColor = Color.FromArgb(pixelColor.R, 0, 0);
                    //image1.SetPixel(x, y, newColor);
                }
                if (point == -2)
                    break;
            }
            if (point != -2)
            {
                MessageBox.Show("Not Found");
                return;
            }

            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    a[i, j] = "el";
                    freeze[i, j] = 1;
                    stable[i, j] = 0;
                }
            }

            timer1.Start();
        }
        public static int lastMaxScore = 1;
        public static int  count= 0;
        public static int[,] freeze = new int[8, 8];
        public static int[,] stable = new int[8, 8];
        public static string[,] a = new string[8, 8];

        public void readGem()
        {
            string type;
            int i, j;
            int r=0, g=0, b=0;
            Color tmp,tmp2;
            for (i = 7; i >= 0 ; i--)
            {
                for (j = 0; j < 8; j++)
                {
                    type = "el";
                    tmp = table.GetPixel(40 * j + 20, 40 * i + 20);
                    if((tmp.R==bg[i,j,0] && tmp.G==bg[i,j,1] && tmp.B==bg[i,j,2]) || (i+1<8 && a[i+1,j]=="--"))
                    {
                        type="--";
                    }
                    if (type == "el")   //may be multiplyer
                    {
                        tmp = table.GetPixel(40 * j + 13, 40 * i + 21);
                        tmp2 = table.GetPixel(40 * j + 11, 40 * i + 26);
                        if (tmp.R + tmp.G + tmp.B + tmp2.R + tmp2.G + tmp2.B >= (255 * 6) - 3)   
                        {
                            tmp = table.GetPixel(40 * j + 14, 40 * i + 14);
                            r = tmp.R;
                            g = tmp.G;
                            b = tmp.B;
                            if (r > b && b > g)
                            {
                                if (b < 136)
                                    type = "rm";
                                else
                                    type = "pm";
                            }
                            else if (b > g && g > r)
                                type = "bm";
                            else if (g > b && b > r)
                                type = "gm";
                            else if ((g > r && r > b) || (r > g && g > b))
                            {
                                if (g > 190)
                                    type = "ym";
                                else
                                    type = "om";
                            }
                            else if (Math.Abs(203 - r) < 5 && Math.Abs(203 - g) < 5 && Math.Abs(203 - b) < 5)
                                type = "wm";
                        }
                    }
                    if (type == "el")   //may be type _0 or _1
                    {
                        tmp = table.GetPixel(40 * j + 20, 40 * i + 20);
                        r = tmp.R;
                        g = tmp.G;
                        b = tmp.B;

                        if (r == 249 && g == 26 && b == 54)
                            type= "r0";
                        else if (r == 250 && g == 29 && b == 56)
                            type= "r1";

                        else if (r == 239 && g == 15 && b == 239)
                            type= "p0";
                        else if (r == 240 && g == 18 && b == 240)
                            type= "p1";

                        else if (r == 16 && g == 139 && b == 254)
                            type= "b0";
                        else if (r == 19 && g == 139 && b == 255)
                            type= "b1";

                        else if (r == 224 && g == 224 && b == 224)
                            type= "w0";
                        else if (r == 232 && g == 232 && b == 232)
                            type= "w1";

                        else if ((r == 254 && g == 245 && b == 35) || (r==186 && g==145 && b==34))
                            type= "y0";
                        else if (r == 255 && g == 246 && b == 37)
                            type= "y1";

                        else if (r == 16 && g == 164 && b == 33)
                            type= "g0";
                        else if (r == 21 && g == 167 && b == 38)
                            type= "g1";

                        else if (r == 230 && g == 101 && b == 33)
                            type= "o0";
                        else if (r == 231 && g == 107 && b == 37)
                            type= "o1";
                    }
                    if (type == "el") //may be hypercube
                    {
                        int min = 255 * 3, sum;
                        for (int run = 14; run < 30; run++)
                        {
                            tmp = table.GetPixel(40 * j + 20, 40 * i + run);
                            sum = tmp.R + tmp.G + tmp.B;
                            if (sum < min)
                            {
                                min = sum;
                                if (min < 150)  //hypercube max now at 105
                                {
                                    type = "hy";
                                    break;
                                }
                            }
                        }
                        //richTextBox1.AppendText(min.ToString());
                    }
                    if (type == "el") //may be type _2
                    {
                        tmp = table.GetPixel(40 * j + 18, 40 * i + 18);
                        type = classify2(r, g, b, tmp.R, tmp.G, tmp.B);
                    }

                    //richTextBox1.AppendText(type + "\t");
                    if (type != a[i, j] || type=="--" || type=="el")
                    {
                        stable[i, j] = 0;
                    }
                    else if (stable[i, j] < sc)
                    {
                        stable[i, j]++;
                    }
                    a[i, j] = type;
                }
                //richTextBox1.AppendText("\n");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            count++;
                
            int i, j;
            richTextBox1.Clear();

            gg.CopyFromScreen(sx, sy, 0, 0, table_size);
            //table.Save("abc.bmp", System.Drawing.Imaging.ImageFormat.Bmp);            

            readGem();
            

            //bool hit=false;
            all.Clear();

            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (a[i, j] != last[i, j])
                    {
                        last[i, j] = a[i, j];
                        richTextBox1.AppendText(a[i, j] + "X\t");
                    }
                    else
                    {
                        richTextBox1.AppendText(a[i, j] + "A\t");
                    }
                    if (freeze[i, j] > 0)
                        freeze[i, j]--;
                }
                richTextBox1.AppendText("\n");
            }

            //timer1.Stop();
            //return;

            Color tmp = table.GetPixel(30, 180);
                
            
            if ((count * timer1.Interval > 60000 && tmp.R==83 && tmp.G==4 && tmp.B==4) || count * timer1.Interval > 80000)
            {
                timer1.Stop();
                count = 0;
                //richTextBox1.Clear();
                richTextBox1.AppendText("\nend game");
                return;
            }
            
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (freeze[i, j] == 0 && stable[i,j]==sc && a[i,j][0]!='e' && a[i,j][0]!='h')
                    {
                        if (i - 1 >= 0 && j - 1 >= 0 && j + 1 < 8 && a[i, j][0] == a[i - 1, j - 1][0] && a[i, j][0] == a[i - 1, j + 1][0]
                            && freeze[i - 1, j - 1] == 0 && freeze[i - 1, j + 1]==0)
                        {
                            save(i, j, i - 1, j,1); //move up
                        }
                        else if (i + 1 < 8 && j - 1 >= 0 && j + 1 < 8 && a[i, j][0] == a[i + 1, j - 1][0] && a[i, j][0] == a[i + 1, j + 1][0]
                            && freeze[i + 1, j - 1] == 0 && freeze[i + 1, j + 1]==0)
                        {
                            save(i, j, i + 1, j,2); //move down
                        }
                        else if (i - 1 >= 0 && j - 1 >= 0 && i + 1 < 8 && a[i, j][0] == a[i - 1, j - 1][0] && a[i, j][0] == a[i + 1, j - 1][0]
                            && freeze[i - 1, j - 1] == 0 && freeze[i + 1, j - 1] == 0)
                        {
                            save(i, j, i, j - 1,3); //move left
                        }
                        else if (i + 1 < 8 && i - 1 >= 0 && j + 1 < 8 && a[i, j][0] == a[i - 1, j + 1][0] && a[i, j][0] == a[i + 1, j + 1][0]
                            && freeze[i - 1, j + 1] == 0 && freeze[i + 1, j + 1] == 0)
                        {
                            save(i, j, i, j + 1,4); //move right
                        }
                    }
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 7; j++)
                {
                    if (a[i, j][0] == a[i, j + 1][0] && stable[i,j]==sc && stable[i,j+1]==sc && a[i, j][0] != 'e' && a[i, j][0] != 'h'
                        && freeze[i, j] == 0 && freeze[i , j + 1] == 0)
                    {
                        if (i - 1 >= 0 && j - 1 >= 0 && a[i - 1, j - 1][0] == a[i, j][0]  && freeze[i-1,j-1]==0)
                        {
                            save(i - 1, j - 1, i, j - 1,5);
                        }
                        else if (j - 2 >= 0 && a[i, j - 2][0] == a[i, j][0])
                        {
                            save(i, j - 1, i, j - 2,6);
                        }
                        else if (i + 1 < 8 && j - 1 >= 0 && a[i + 1, j - 1][0] == a[i, j][0] && freeze[i + 1, j - 1] == 0)
                        {
                            save(i, j - 1, i + 1, j - 1,7);
                        }
                        else if (i - 1 >= 0 && j + 2 < 8 && a[i - 1, j + 2][0] == a[i, j][0] && freeze[i - 1, j +2] == 0)
                        {
                            save(i, j + 2, i - 1, j + 2,8);
                        }
                        else if (j + 3 < 8 && a[i, j + 3][0] == a[i, j][0] && freeze[i, j +3] == 0)
                        {
                            save(i, j + 2, i, j + 3,9);
                        }
                        else if (i + 1 < 8 && j + 2 < 8 && a[i + 1, j + 2][0] == a[i, j][0] && freeze[i+1, j + 2] == 0)
                        {
                            save(i, j + 2, i + 1, j + 2,10);
                        }
                    }
                }
            }
            
            for (i = 0; i < 7; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if(a[i, j][0] == a[i + 1, j][0] && stable[i, j] == sc && stable[i + 1, j] == sc && a[i, j][0] != 'e' && a[i, j][0] != 'h'
                        && freeze[i , j ] == 0 && freeze[i +1, j] == 0)
                    {
                        if (i - 1 >= 0 && j - 1 >= 0 && a[i - 1, j - 1][0] == a[i, j][0] && freeze[i - 1, j - 1] == 0)
                        {
                            save(i - 1, j - 1, i - 1, j,11);
                        }
                        else if (i - 2 >= 0 && a[i - 2, j][0] == a[i, j][0] && freeze[i - 2, j] == 0)
                        {
                            save(i - 2, j, i - 1, j,12);
                        }
                        else if (i - 1 >= 0 && j + 1 < 8 && a[i - 1, j + 1][0] == a[i, j][0] && freeze[i - 1, j + 1] == 0)
                        {
                            save(i - 1, j + 1, i - 1, j,13);
                        }
                        else if (i + 2 < 8 && j - 1 >= 0 && a[i + 2, j - 1][0] == a[i, j][0] && freeze[i + 2, j - 1] == 0)
                        {
                            save(i + 2, j - 1, i + 2, j,14);
                        }
                        else if (i + 3 < 8 && a[i + 3, j][0] == a[i, j][0] && freeze[i + 3, j] == 0)
                        {
                            save(i + 3, j, i + 2, j,15);
                        }
                        else if (i + 2 < 8 && j + 1 < 8 && a[i + 2, j + 1][0] == a[i, j][0] && freeze[i + 2, j + 1] == 0)
                        {
                            save(i + 2, j + 1, i + 2, j,16);
                        }
                    }
                }
            }
            // save all except hypercube
            foreach (result v in all)
            {
                v.calculate(a);
            }
            all.Sort(delegate(result p1, result p2)
            {
                return p2.score.CompareTo(p1.score);
            });
            int modifyer = 1;
            int area=0;
            if (all.Count > 0 && all[0].score>=3)
            {
                label1.Text = all[0].score.ToString();
                /*
                if (all.Count > 1)
                {
                    if (all[0].score >= all[1].score)
                        label1.Text = all[0].score.ToString() + " " + all[1].score.ToString();
                    else
                        label1.Text = "?????????";
                }*/
                if (all[0].score == lastMaxScore)
                {
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (all[0].zone[i, j] == 2)
                                area++;
                        }
                    }
                    if (area >= 10)
                        modifyer = 1;
                    for (i = 0; i < 8; i++)
                    {
                        for (j = 0; j < 8; j++)
                        {
                            if (all[0].zone[i, j] == 2)
                                freeze[i, j] = fc * modifyer;
                        }
                    }
                    freeze[all[0].i1, all[0].j1] = fc;
                    freeze[all[0].i2, all[0].j2] = fc;
                    /*  //debug
                    if (all[0].a[all[0].i1, all[0].j1][0] == all[0].a[all[0].i2, all[0].j2][0])
                    {
                        timer1.Stop();
                        modifyer = 1;
                    }*/
                    swap(all[0].i1, all[0].j1, all[0].i2, all[0].j2);       //bigdeal
                    if (all.Count > 1)
                    {
                        lastMaxScore = all[1].score;
                    }
                }
                else
                {
                    lastMaxScore = all[0].score;
                }
            }
            /*
            for (i = 0; i < all.Count; i++)
            {

            }*/


            if (all.Count == 0)   //destroy hypercube || all[0].score<1000
            {                
                Dictionary<char, int> d = new Dictionary<char, int>();
                d.Add('r',0);
                d.Add('g',0);
                d.Add('b',0);
                d.Add('w',0);
                d.Add('p',0);
                d.Add('y',0);
                d.Add('o',0);
                
                d.Add('h',0);
                d.Add('e',0);
                d.Add('-',0);

                for(i=0;i<8;i++)
                {
                    for(j=0;j<8;j++)
                    {
                        d[a[i,j][0]]++;
                    }
                }
                d['h'] = 0;
                d['e'] = 0;
                d['-'] = 0;
                for(i=0;i<8;i++)
                {
                    for(j=0;j<8;j++)
                    {
                        if (a[i, j] == "hy")
                        {
                            if (i - 1 >= 0
                                && (i + 1 >= 8||d[a[i - 1, j][0]] >= d[a[i + 1, j][0]]  )
                                && (j + 1 >= 8||d[a[i - 1, j][0]] >= d[a[i, j + 1][0]]  )
                                && (j - 1 < 0||d[a[i - 1, j][0]] >= d[a[i, j - 1][0]]  )
                              )
                                swap(i, j, i - 1, j);
                            else if (i + 1 <8
                                && (i - 1 < 0||d[a[i + 1, j][0]] >= d[a[i - 1, j][0]]  )
                                && (j + 1 >= 8||d[a[i + 1, j][0]] >= d[a[i, j + 1][0]]  )
                                && (j - 1 < 0||d[a[i + 1, j][0]] >= d[a[i, j - 1][0]]  )
                              )
                                swap(i, j, i + 1, j);
                            else if (j - 1 >= 0
                                && (i + 1 >= 8||d[a[i , j- 1][0]] >= d[a[i + 1, j][0]]  )
                                && (j + 1 >= 8||d[a[i , j- 1][0]] >= d[a[i, j + 1][0]]  )
                                && (i - 1 < 0||d[a[i , j- 1][0]] >= d[a[i - 1, j][0]]  )
                              )
                                swap(i, j, i , j- 1);
                            else if (j+1<8
                                && (i + 1 >= 8||d[a[i , j+ 1][0]] >= d[a[i + 1, j][0]]  )
                                && (i - 1 <0||d[a[i , j+ 1][0]] >= d[a[i - 1, j][0]]  )
                                && (j - 1 < 0||d[a[i , j+ 1][0]] >= d[a[i, j - 1][0]]  )
                              )
                                swap(i, j, i , j+ 1);
                            return;
                        }
                    }
                }
            }
            
        }

        public List<result> all;
        public void save(int i1, int j1, int i2, int j2,int debug_code)
        {
            all.Add(new result(i1,j1,i2,j2,debug_code));
        }

        public void swap(int i1,int j1,int i2,int j2)
        {
            //Thread.Sleep(5);
            /*
            if (i1 > 3)
            {
                doClick(sx + 20, sy + 20);
            }
            else
            {
                doClick(sx + 7 * 40 + 20, sy + 7 * 40 + 20);
            }
            */
            Cursor.Position = new Point(sx + j1 * 40 + 20, sy + i1 * 40 + 20);
            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
            Cursor.Position = new Point(sx + j2 * 40 + 20, sy + i2 * 40 + 20);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
            
            stable[i1, j1] = 0;
            stable[i2, j2] = 0;
            /*
            if (i1 > 3)
            {
                doClick(sx + 20, sy + 20);
                doClick(sx + 20, sy + 20);
            }
            else
            {
                doClick(sx + 7 * 40 + 20, sy + 7 * 40 + 20);
                doClick(sx + 7 * 40 + 20, sy + 7 * 40 + 20);
            }
            */
            //Thread.Sleep(5);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            timer1.Stop();
        }
    }

    public class result
    {

        public static int[,] d = new int[4, 2] {{1,0},{-1,0},{0,1},{0,-1}};
        public int i1, j1, i2, j2;
        public string[,] a;
        public int[,] zone;
        public int score;
        public int debug_code;
        public result(int i1,int j1,int i2,int j2,int debug_code)
        {
            this.i1 = i1;
            this.i2 = i2;
            this.j1 = j1;
            this.j2 = j2;
            this.debug_code = debug_code;
            a = new string[8, 8];
            zone = new int[8, 8];
        }
        public void calculate(string[,] b)  //make it easy now  (no combo)
        {
        
            int i, j, k;
            int r;
            
            
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    zone[i, j] = 0;
                    a[i, j] = b[i, j];
                }
            }
            
            //swap
            string tmp;
            tmp = a[i1, j1];
            a[i1, j1] = a[i2, j2];
            a[i2, j2] = tmp;
            
            //run
            score = 0;

            if (a[i1, j1][0] == a[i2, j2][0])
            {
                score = -1;
                return;
            }

            for (int z = 0; z < 2; z++)
            {
                if (z == 0)
                {
                    i = i1;
                    j = j1;
                }
                else
                {
                    i = i2;
                    j = j2;
                }

                int[] gem = new int[4] { 0, 0, 0, 0 };
                for (r = 0; r < 4; r++)
                {
                    for (k = 1; (i + d[r, 0] * k >= 0 && i + d[r, 0] * k < 8 && j + d[r, 1] * k >= 0 && j + d[r, 1] * k < 8); k++)
                    {
                        if (a[i + d[r, 0] * k, j + d[r, 1] * k][0] == a[i, j][0])
                            gem[r]++;
                        else
                            break;
                    }
                }

                if (gem[0] + gem[1] >= 2 && gem[2] + gem[3] >= 2)   //make a star gem
                {
                    score += 40000;
                }
                else if (gem[0] + gem[1] >= 4)  //hypercube (vertical)
                {
                    score += 50000;
                }
                else if (gem[0] + gem[1] >= 3)  //flame (vertical)
                {
                    score += 20000;
                }
                else if (gem[2] + gem[3] >= 4)  //hypercube (horizontal)
                {
                    score += 50000;
                }
                else if (gem[2] + gem[3] >= 3)  //flame (horizontal)
                {
                    score += 20000;
                }

                if (gem[0] + gem[1] >= 2)
                {
                    for (r = 0; r < 2; r++)
                    {
                        for (k = 1; k<= gem[r] ; k++)
                        {
                            zone[i + d[r, 0] * k, j + d[r, 1] * k]=1;
                        }
                    }
                }
                if (gem[2] + gem[3] >= 2)
                {
                    for (r = 2; r < 4; r++)
                    {
                        for (k = 1; k <= gem[r]; k++)
                        {
                            zone[i + d[r, 0] * k, j + d[r, 1] * k]=1;
                        }
                    }
                }
                if (gem[0] + gem[1] >= 2 || gem[2] + gem[3] >= 2)
                    zone[i, j]=1;
            }
            bool newzone=true;
            while (newzone)
            {
                newzone=false;
                for (i = 0; i < 8; i++)
                {
                    for (j = 0; j < 8; j++)
                    {
                        if (zone[i, j] == 1)
                        {
                            zone[i, j] = 2;
                            if (a[i, j][1] == '2')          //star
                            {
                                newzone = true;
                                for (k = 0; k < 8; k++)
                                {
                                    if (zone[i, k] == 0)
                                        zone[i, k] = 1;
                                    if (zone[k, j] == 0)
                                        zone[k, j] = 1;
                                }
                            }
                            else if (a[i, j][1] == '1')
                            {
                                newzone = true;
                                int ii,jj;
                                for (ii = -1; ii <= 1; ii++)
                                {
                                    for (jj = -1; jj <= 1; jj++)
                                    {
                                        if (i + ii >= 0 && i + ii < 8 && j + jj >= 0 && j + jj < 8 && zone[i + ii, j + jj] == 0)
                                            zone[i + ii, j + jj] = 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            for (i = 0; i < 8; i++)
            {
                for (j = 0; j < 8; j++)
                {
                    if (zone[i, j] == 2)
                    {
                        score += getCost(a[i, j]);
                    }
                }
            }
        }
        public int getCost(string s)
        {
            if (s[1] == 'm')
                return 100000;
            if (s[1] == '2')
                return 10000;
            if (s[1] == '1')
                return 500;
            if (s == "hy")
                return 1000;
            return 1;
        }


    }
    
}
