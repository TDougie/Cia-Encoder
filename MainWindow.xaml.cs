using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using Microsoft.Win32;


namespace Cia_Encoder {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public static string _Path = "";
        public static string newPath = "";
        public static int _width = 0;
        public static int _height = 0;
        public static string _pixelePalette = "";
        public static Color[] _aryColors;
        public static BitmapMaker _Bmp;
        public static string _message;
        public MainWindow() {
            InitializeComponent();
        }
        private void muOpen_Click(object sender, RoutedEventArgs e) {
            //creat open file dialog
            OpenFileDialog OpenFileDialog = new OpenFileDialog();

            //setup parameters for our open fiel dialog
            OpenFileDialog.DefaultExt = ".ppm";
            OpenFileDialog.Filter = "PPM Files (.ppm)|*.ppm";

            //SHOW FILE DIALOG
            bool? result = OpenFileDialog.ShowDialog();

            //int num = null; can't be done because "int" is a value type
            //int? num = null; question mark allows a value type to be set to null. Only reference types can be set to null
            //ShowDialog has to be stored to a nullable bool.

            //PROCESS DIALOG RESULTS
            if (result == true) {
                _Path = OpenFileDialog.FileName; //FileName sotres the filepath to this property. 

                //CALL LOADIMAGE IF A FILE HAS BEEN SELECTED

                ShowPpmImage(_Path);
            }//end if


        }//end event


        //}//end LoadImage
        private void ShowPpmImage(string path) {
            //OPEN THE PPM IMAGE
            StreamReader infile = new StreamReader(path);

            //READ HEADER
            //use where(val == "example") to stop a streamread at a certain text or character
            string header = infile.ReadLine();
            string words = infile.ReadLine();
            string dimension = infile.ReadLine();
            string maxColor = infile.ReadLine();

            if (header == "P3") {
                //PROCESS PIXEL DATA
                string pixelPalette = infile.ReadToEnd();
                _pixelePalette = pixelPalette;
                infile.Close();

                //PROCESS HEADER DIMENSIONS

                string[] aryDimensions = dimension.Split();
                string[] aryPalette = pixelPalette.Split();

                int width = int.Parse(aryDimensions[0]);
                int height = int.Parse(aryDimensions[1]);
                _width = width;
                _height = height;
                //PROCESS HEADER PALTTE DATA
                //FOR P6 CAST TO CHAR ARRAY AND PROCESS IT IN IT'S BIT FORM

                Color[] aryColors = new Color[aryPalette.Length / 3];


                for (int paletteIndex = 0; paletteIndex * 3 < aryPalette.Length - 1; paletteIndex++) {
                    int newIndex = paletteIndex * 3;

                    aryColors[paletteIndex].A = byte.Parse(maxColor);
                    aryColors[paletteIndex].R = byte.Parse(aryPalette[newIndex]);
                    aryColors[paletteIndex].G = byte.Parse(aryPalette[++newIndex]);
                    aryColors[paletteIndex].B = byte.Parse(aryPalette[++newIndex]);
                }//end for
                _aryColors = aryColors;
                //CREATE A BITMAPMAKER TO HOLD IMAGE DATA
                BitmapMaker bmpMaker = new BitmapMaker(width, height);

                int plotX = 0;
                int plotY = 0;
                int colorIndex = 0;
                //LOOPING THORUGH PIXEL DATA TO SET THE PIXELS
                for (int index = 0; index < aryPalette.Length; index++) {

                    Color plotColor = aryColors[colorIndex];

                    bmpMaker.SetPixel(plotX, plotY, plotColor);

                    plotX++;
                    if (plotX == width) {
                        plotX = 0;
                        plotY += 1;
                    }// end if
                    if (plotY == height) {
                        break;
                    }
                    colorIndex++;
                }//end for

                //CREATE NEW BITMAP
                _Bmp = bmpMaker;
                WriteableBitmap wbmImage = bmpMaker.MakeBitmap();

                //SET IMAGE CONTROL TO DISPLAY THE BITMAP
                imgMain.Source = wbmImage;
            } else { //end p3 start p6
                infile.Close();
                FileStream fs = File.OpenRead(path);
                byte currentByte = (byte)fs.ReadByte();
                byte lf = 10;
                string[] fileData = new string[4];
                for (int i = 0; i < fileData.Length; i++) {
                    while (currentByte != lf) {
                        fileData[i] += (char)currentByte;
                        currentByte = (byte)fs.ReadByte();
                    }
                    currentByte = (byte)fs.ReadByte();
                }

                string pixelPalette = "";

                pixelPalette += (char)currentByte;
                while (fs.Position < fs.Length) {
                    //PROCESS PIXEL DATA
                    pixelPalette += (char)fs.ReadByte();
                }
                _pixelePalette = pixelPalette;
                fs.Close();
                string[] aryDimension = dimension.Split();
                int width = int.Parse(aryDimension[0]);
                int height = int.Parse(aryDimension[1]);
                _width = width;
                _height = height;
                Color[] aryColors = new Color[pixelPalette.Length / 3];
                for (int paletteIndex = 0; paletteIndex * 3 < pixelPalette.Length - 1; paletteIndex++) {
                    int newIndex = paletteIndex * 3;
                    aryColors[paletteIndex].A = byte.Parse(maxColor);
                    aryColors[paletteIndex].R = (byte)pixelPalette[newIndex];
                    aryColors[paletteIndex].G = (byte)pixelPalette[++newIndex];
                    aryColors[paletteIndex].B = (byte)pixelPalette[++newIndex];
                }
                _aryColors = aryColors;

                BitmapMaker bmpMaker = new BitmapMaker(width, height); int plotX = 0; int plotY = 0;
                int colorIndex = 0;
                for (int index = 0; index < pixelPalette.Length; index++) {
                    Color plotColor = aryColors[colorIndex];
                    bmpMaker.SetPixel(plotX, plotY, plotColor);
                    plotX++; if (plotX == width) {
                        plotX = 0;
                        plotY += 1;
                    }// end if
                    if (plotY == height) {
                        break;
                    }
                    colorIndex++;
                }//end for
                _Bmp = bmpMaker;
                WriteableBitmap wbmImage = bmpMaker.MakeBitmap();

                //SET IMAGE CONTROL TO DISPLAY THE BITMAP
                imgMain.Source = wbmImage;
                //lsb look it up for encoding
            }
        }//end ShowPpmImage





        private void TxtBoxPic1_TextChanged(object sender, TextChangedEventArgs e) {

        }//end TxtBoxPic1

        private void btnEncode_Click(object sender, RoutedEventArgs e) {
            _message = TxtBoxMesage.Text;

            EncodeMessage(_Bmp, _message);
            WriteableBitmap wbmImage = _Bmp.MakeBitmap();
            imgMain2.Source = wbmImage;

        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            SaveAsPpm();
        }//end save

       
        public static void EncodeMessage(BitmapMaker bmpMaker, string message) {
            // Convert message to binary string
            StringBuilder binaryMessage = new StringBuilder();
            foreach (char c in message) {
                binaryMessage.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }

            // Add end of text signal and padding zeros
            int paddingCount = (binaryMessage.Length + 8) % 24;
            binaryMessage.Append("00001000".PadRight(paddingCount, '0'));
            binaryMessage.Append(paddingCount.ToString("X2"));

            // Encode message into image
            int messageIndex = 0;
            for (int y = 0; y < bmpMaker.Height; y++) {
                for (int x = 0; x < bmpMaker.Width; x++) {
                    // Get pixel color
                    Color pixelColor = bmpMaker.GetPixelColor(x, y);

                    // Encode message bits into color channels
                    if (messageIndex < binaryMessage.Length) {
                        byte r = (byte)(pixelColor.R & 0xFE | (binaryMessage[messageIndex++] - '0'));
                        if (messageIndex < binaryMessage.Length) {
                            byte g = (byte)(pixelColor.G & 0xFE | (binaryMessage[messageIndex++] - '0'));
                            if (messageIndex < binaryMessage.Length) {
                                byte b = (byte)(pixelColor.B & 0xFE | (binaryMessage[messageIndex++] - '0'));
                                bmpMaker.SetPixel(x, y, Color.FromRgb(r, g, b));
                            } else {
                                bmpMaker.SetPixel(x, y, Color.FromRgb(r, (byte)(pixelColor.G & 0xFE), pixelColor.B));
                            }
                        } else {
                            bmpMaker.SetPixel(x, y, Color.FromRgb(r, pixelColor.G, pixelColor.B));
                        }
                    } else {
                        // No more message bits to encode, exit loop
                        _Bmp = bmpMaker;
                        break;
                    }
                }
                if (messageIndex >= binaryMessage.Length) {
                    // No more message bits to encode, exit loop
                    break;
                }
            }
        }

        public static void SaveAsPpm() {
            Random rnd = new Random();
            int num = rnd.Next(0, 1000);
            string filename = "image_" + num + ".ppm";

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = filename;
            saveFileDialog.Filter = "PPM file (.ppm)|.ppm";

            if (saveFileDialog.ShowDialog() == true) {
                using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName)) {
                    // Write header
                    writer.WriteLine("P3");
                    writer.WriteLine("# Created by my program");
                    writer.WriteLine($"{_width} {_height}");
                    writer.WriteLine("255");

                    // Write pixel data
                    for (int y = 0; y < _Bmp.Height; y++) {
                        for (int x = 0; x < _Bmp.Width; x++) {
                            Color color = _Bmp.GetPixelColor(x, y);
                            writer.WriteLine($"{color.R}");
                            writer.WriteLine($"{color.G}");
                            writer.WriteLine($"{color.B}");
                        }
                    }
                }//end using
            }//end if
        }//end save


    }//end main
}//end namespace
