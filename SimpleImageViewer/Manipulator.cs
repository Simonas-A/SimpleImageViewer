using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimpleImageViewer
{
    public static class Manipulator
    {
        public static Rectangle GetDisplayedRectangle(Image img, Rectangle resolution, PictureBoxSizeMode sizeMode)
        {
            int x;
            int y;
            int width;
            int height;

            if ((img.Width < resolution.Width && img.Height < resolution.Height) && !(sizeMode == PictureBoxSizeMode.Zoom))
            {
                x = Convert.ToInt32(resolution.Width * 0.5 - img.Width * 0.5);
                y = Convert.ToInt32(resolution.Height * 0.5 - img.Height * 0.5);
                width = img.Width;
                height = img.Height;
            }
            else
            {
                if ((double)img.Width / resolution.Width > (double)img.Height / resolution.Height)
                {
                    double ratio = (double)img.Width / resolution.Width;
                    x = 0;
                    y = Convert.ToInt32((resolution.Height - (img.Height / ratio)) * 0.5);
                    width = resolution.Width;
                    height = Convert.ToInt32(img.Height / ratio);
                }
                else
                {
                    double ratio = (double)img.Height / resolution.Height;
                    x = Convert.ToInt32((resolution.Width - (img.Width / ratio)) * 0.5);
                    y = 0;
                    width = Convert.ToInt32(img.Width / ratio);
                    height = resolution.Height;
                }
            }
            return new Rectangle(x, y, width, height);
        }

        public static Rectangle GetRectangle(Rectangle oldDisplayedRectangle, Rectangle resolution, int delta, int eX, int eY, Size baseImageRectangle, double scale)
        {
            double zoomFactor = 0.85;
            double displayRatio = (double)resolution.Width / resolution.Height;

            int width, height;

            if ((double)baseImageRectangle.Width / resolution.Width > (double)baseImageRectangle.Height / resolution.Height)
            {//Wide picture

                width = Convert.ToInt32(baseImageRectangle.Width * Math.Pow(zoomFactor, scale));
                height = Convert.ToInt32(width / displayRatio);

            }
            else
            {//Tall picture

                height = Convert.ToInt32(baseImageRectangle.Height * Math.Pow(zoomFactor, scale));
                width = Convert.ToInt32(height * displayRatio);
            }

            int x = oldDisplayedRectangle.X + ((oldDisplayedRectangle.Width - width)) * eX / (resolution.Width);
            int y = oldDisplayedRectangle.Y + ((oldDisplayedRectangle.Height - height)) * eY / (resolution.Height);


            if (delta < 0)
            {

                if (x + width > baseImageRectangle.Width)
                {
                    x = baseImageRectangle.Width - width;
                }

                if (y + height > baseImageRectangle.Height)
                {
                    y = baseImageRectangle.Height - height;
                }

                if (x < 0)
                {
                    x = 0;

                    if (x + width > baseImageRectangle.Width)
                    {
                        x = (baseImageRectangle.Width - width) / 2;
                    }
                }

                if (y < 0)
                {
                    y = 0;

                    if (y + height > baseImageRectangle.Height)
                    {
                        y = (baseImageRectangle.Height - height) / 2;
                    }
                }
            }
            else
            {
                if (x < 0 || x + width > baseImageRectangle.Width)
                {
                    x = 0;
                    width = baseImageRectangle.Width;
                }

                if (y < 0 || y + height > baseImageRectangle.Height)
                {
                    y = 0;
                    height = baseImageRectangle.Height;
                }
            }

            return new Rectangle(x, y, width, height);
        }

        public static Image ZoomImage(int delta, int eX, int eY, Image baseImage, ref Rectangle oldDisplayedRectangle, int scale, Rectangle resolution, int sizeReduceFactor)
        {
            Console.WriteLine("zvoom");
            double zoomFactor = 0.85;

            /*
            if (scale <= 0)
            {
                scale = 0;
                pictureBox.Image = baseImage;
                oldDisplayedRectangle = new Rectangle(0, 0, baseImage.Width, baseImage.Height);

                return baseImage;
            }
            */
            //Rectangle displayRectangle = GetDisplayedRectangle(pictureBox.Image, resolution, pictureBox.SizeMode);


            double displayRatio = (double)resolution.Width / resolution.Height;

            int width, height;

            bool widePicture = true;

            if ((double)baseImage.Width / resolution.Width > (double)baseImage.Height / resolution.Height)
            {//Wide picture

                width = Convert.ToInt32(baseImage.Width * Math.Pow(zoomFactor, scale));
                height = Convert.ToInt32(width / displayRatio);

            }
            else
            {//Tall picture

                widePicture = false;
                height = Convert.ToInt32(baseImage.Height * Math.Pow(zoomFactor, scale));
                width = Convert.ToInt32(height * displayRatio);
            }

            int x = oldDisplayedRectangle.X + ((oldDisplayedRectangle.Width - width)) * eX / (resolution.Width);
            int y = oldDisplayedRectangle.Y + ((oldDisplayedRectangle.Height - height)) * eY / (resolution.Height);

            
            if (delta < 0)
            {

                if (x + width > baseImage.Width)
                {
                    x = baseImage.Width - width;
                }

                if (y + height > baseImage.Height)
                {
                    y = baseImage.Height - height;
                }

                if (x < 0)
                {
                    x = 0;

                    if (x + width > baseImage.Width)
                    {
                        x = (baseImage.Width - width) / 2;
                    }
                }

                if (y < 0)
                {
                    y = 0;

                    if (y + height > baseImage.Height)
                    {
                        y = (baseImage.Height - height) / 2;
                    }
                }
            }
            else
            {
                if (x < 0 || x + width > baseImage.Width)
                {
                    x = 0;
                    width = baseImage.Width;
                }

                if (y < 0 || y + height > baseImage.Height)
                {
                    y = 0;
                    height = baseImage.Height;
                }
            }
            

            oldDisplayedRectangle = new Rectangle(x, y, width, height);


            int displayWidth, displayHeight;
            if (widePicture)
            {
                displayWidth = resolution.Width;
                displayHeight = height * resolution.Width / width;
            }
            else
            {
                displayHeight = resolution.Height;
                displayWidth = width * resolution.Height / height;
            }

            displayHeight /= sizeReduceFactor;
            displayWidth /= sizeReduceFactor;

            Bitmap bmp = new Bitmap(displayWidth, displayHeight);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(baseImage, new Rectangle(0, 0, displayWidth, displayHeight), oldDisplayedRectangle, GraphicsUnit.Pixel);
            }

            //baseImage.Dispose();

            return bmp;
        }

        public static Image ZoomImage(Image baseImage, Rectangle oldDisplayedRectangle, Rectangle resolution, int sizeReduceFactor)
        {
            //Console.WriteLine("zvoom");

            bool widePicture = true;

            if ((double)baseImage.Width / resolution.Width > (double)baseImage.Height / resolution.Height)
            {//Wide picture

            }
            else
            {//Tall picture

                widePicture = false;
            }

            int displayWidth, displayHeight;


            if (widePicture)
            {
                displayWidth = resolution.Width;
                displayHeight = oldDisplayedRectangle.Height * resolution.Width / oldDisplayedRectangle.Width;
            }
            else
            {
                displayHeight = resolution.Height;
                displayWidth = oldDisplayedRectangle.Width * resolution.Height / oldDisplayedRectangle.Height;
            }

            displayHeight /= sizeReduceFactor;
            displayWidth /= sizeReduceFactor;

            Bitmap bmp = new Bitmap(displayWidth, displayHeight);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighSpeed;
                g.DrawImage(baseImage, new Rectangle(0, 0, displayWidth, displayHeight), oldDisplayedRectangle, GraphicsUnit.Pixel);
            }

            //baseImage.Dispose();

            return bmp;
        }

        public static Image ZoomInImage(int eX, int eY, Image baseImage, Rectangle resolution, int sizeReduceFactor)
        {
            double zoomFactor = 0.85;

            /*
            if (scale <= 0)
            {
                scale = 0;
                pictureBox.Image = baseImage;
                oldDisplayedRectangle = new Rectangle(0, 0, baseImage.Width, baseImage.Height);

                return baseImage;
            }
            */
            //Rectangle displayRectangle = GetDisplayedRectangle(pictureBox.Image, resolution, pictureBox.SizeMode);


            double displayRatio = (double)resolution.Width / resolution.Height;

            int width, height;

            bool widePicture = true;

            if ((double)baseImage.Width / resolution.Width > (double)baseImage.Height / resolution.Height)
            {//Wide picture

                width = Convert.ToInt32(baseImage.Width * zoomFactor);
                height = Convert.ToInt32(width / displayRatio);

            }
            else
            {//Tall picture

                widePicture = false;
                height = Convert.ToInt32(baseImage.Height * zoomFactor);
                width = Convert.ToInt32(height * displayRatio);
            }

            int x = ((baseImage.Width - width)) * eX / (resolution.Width);
            int y = ((baseImage.Height - height)) * eY / (resolution.Height);

            if (x < 0 || x + width > baseImage.Width)
            {
                x = 0;
                width = baseImage.Width;
            }

            if (y < 0 || y + height > baseImage.Height)
            {
                y = 0;
                height = baseImage.Height;
            }
            


            //oldDisplayedRectangle = new Rectangle(x, y, width, height);


            int displayWidth, displayHeight;
            if (widePicture)
            {
                displayWidth = resolution.Width;
                displayHeight = height * resolution.Width / width;
            }
            else
            {
                displayHeight = resolution.Height;
                displayWidth = width * resolution.Height / height;
            }

            displayHeight /= sizeReduceFactor;
            displayWidth /= sizeReduceFactor;

            Bitmap bmp = new Bitmap(displayWidth, displayHeight);

            using (Graphics g = Graphics.FromImage(bmp))
            {
                g.DrawImage(baseImage, new Rectangle(0, 0, displayWidth, displayHeight), new Rectangle(x, y, width, height), GraphicsUnit.Pixel);
            }

            //baseImage.Dispose();

            return bmp;
        }
     
        public static Rectangle GetZoomedRectangle(int delta, int eX, int eY, int baseWidth, int baseHeight, Rectangle oldDisplayedRectangle, double scale, Rectangle resolution)
        {
            double zoomFactor = 0.85;
            double displayRatio = (double)resolution.Width / resolution.Height;
            int width, height;

            if ((double)baseWidth / resolution.Width >= (double)baseHeight / resolution.Height)
            {//Wide picture

                width = Convert.ToInt32(baseWidth * Math.Pow(zoomFactor, scale));
                height = Convert.ToInt32(width / displayRatio);

            }
            else
            {//Tall picture

                height = Convert.ToInt32(baseHeight * Math.Pow(zoomFactor, scale));
                width = Convert.ToInt32(height * displayRatio);
            }

            int x = oldDisplayedRectangle.X + ((oldDisplayedRectangle.Width - width)) * eX / (resolution.Width);
            int y = oldDisplayedRectangle.Y + ((oldDisplayedRectangle.Height - height)) * eY / (resolution.Height);


            if (delta < 0)
            {

                if (x + width > baseWidth)
                {
                    x = baseWidth - width;
                }

                if (y + height > baseHeight)
                {
                    y = baseHeight - height;
                }

                if (x < 0)
                {
                    x = 0;

                    if (x + width > baseWidth)
                    {
                        x = (baseWidth - width) / 2;
                    }
                }

                if (y < 0)
                {
                    y = 0;

                    if (y + height > baseHeight)
                    {
                        y = (baseHeight - height) / 2;
                    }
                }
            }
            else if (delta > 0)
            {
                if (x < 0 || x + width > baseWidth)
                {
                    x = 0;
                    width = baseWidth;
                }

                if (y < 0 || y + height > baseHeight)
                {
                    y = 0;
                    height = baseHeight;
                }
            }
            else
            {
                if (x < 0)
                {
                    x = 0;
                }

                if (x + width > baseWidth)/// bad
                {
                    x = baseWidth - width;
                }

                if (y < 0)
                {
                    y = 0;
                }

                if (y + height > baseHeight)
                {
                    y = baseHeight - height;
                }
            }


            oldDisplayedRectangle = new Rectangle(x, y, width, height);

            return oldDisplayedRectangle;
        }
    }
}
