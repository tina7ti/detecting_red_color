using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics;
using UnityEngine;
using Vuforia;
using OpenCvSharp;
using UnityEngine.UI;


public class CameraImageAccess : MonoBehaviour
{
    #region PRIVATE_MEMBERS
    private PIXEL_FORMAT mPixelFormat = PIXEL_FORMAT.UNKNOWN_FORMAT;
    private bool mAccessCameraImage = true;
    private bool mFormatRegistered = false;
    private int nbrloop = 0
        ;
    #endregion /// PRIVATE_MEMBERS
    Mat mat = null, rgbFrame, hsvFrame, mask ;
    public Text Text;

    // low and high red color
    int[] low_red = new int[3] {161, 155, 84};
    
    int[] high_red = new int[3] { 179, 255, 255 };

    #region MONOBEHAVIOUR_METHODS
    void Start()
    {
        Text.text = "in start function !!";
#if UNITY_EDITOR
        mPixelFormat = PIXEL_FORMAT.RGBA8888; // Need Grayscale for Editor
#else
      mPixelFormat = PIXEL_FORMAT.RGB888; // Use RGB888 for mobile
#endif

        // Register Vuforia life-cycle callbacks:
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPause);
    }
    #endregion // MONOBEHAVIOUR_METHODS

    #region PRIVATE_METHODS
    private void OnVuforiaStarted()
    {
        Text.text = "in onvuforiaStarted !!";
        // Try register camera image format
        if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
        {
            //UnityEngine.Debug.Log("Successfully registered pixel format " + mPixelFormat.ToString());
            mFormatRegistered = true;
            Text.text = "formatregistred is true !!";
        }
        else
        {
           /* UnityEngine.Debug.LogError(
                "\nFailed to register pixel format: " + mPixelFormat.ToString() +
                "\nThe format may be unsupported by your device." +
                "\nConsider using a different pixel format.\n");
            mFormatRegistered = false;
            Text.text = "formatregistred is false !!";*/
        }
  }
  /// <summary>
  /// Called each time the Vuforia state is updated
  /// </summary>
  void OnTrackablesUpdated()
{
        Text.text = "in the ontrackableupdated function !!";
    if (mFormatRegistered)
    {
        if (mAccessCameraImage)
        {
            Vuforia.Image image = CameraDevice.Instance.GetCameraImage(mPixelFormat);
            Text.text = "before the if of image !!";
            if (image != null)
            {
                /*UnityEngine.Debug.Log(
                    "\nImage Format: " + image.PixelFormat +
                    "\nImage Size:   " + image.Width + "x" + image.Height +
                    "\nBuffer Size:  " + image.BufferWidth + "x" + image.BufferHeight +
                    "\nImage Stride: " + image.Stride + "\n"
                );*/
                    Text.text = "before pixels !!";
                    byte[] pixels = image.Pixels;
                    Text.text = "after pixels!!";
                if (pixels != null && pixels.Length > 0)
                {

                       /* UnityEngine.Debug.Log(
                        "\nimage length = " + pixels.Length + " Image pixels: " +
                        pixels[0] + ", " +
                        pixels[1] + ", " +
                        pixels[2] + ", ...\n");*/

                    if(mat == null)
                    {
                        mat = getImgmat(image, mPixelFormat);
                    }
                        mat.SetArray(0, 0, image.Pixels);
                        Text.text = "after the setaray of mat";
                        if (!mat.Empty())
                        {
                            Text.text = "in the test of !mat.empty()";
                            rgbFrame = getRGBmat(mat);

                            // convert the frame from rgb to hsv
                            hsvFrame = getHSVmat(rgbFrame);
                            //Cv2.ImShow("the mat hsv", hsvFrame);
                            // mask creation
                            mask = getMask(hsvFrame, low_red,high_red);
                            Cv2.ImShow("Red mask", mask);

                            if (!mask.Empty())
                            {
                                Text.text = "the mask is workinggggggggg !!";

                                /*OutputArray labels = new OutputArray(new Mat());
                                int cnn = mask.ConnectedComponents(labels,PixelConnectivity.Connectivity8);
                                
                                Text.text = "nbr of labels = " + cnn; */
                                
                                Text.text += "mask(0,0)= " + mask.GetArray(0,0)[0] + " mask(0,1)= " + mask.GetArray(0, 1)[0] +
                                    "\n le milieu("+mask.Height/2+","+mask.Width/2+")= " + mask.GetArray(mask.Height/2, mask.Width/2)[0];

                                nbrloop++;
                                if(nbrloop == 200)
                                {
                                    Text.text += "\n nbrloop = " + nbrloop;
                                    Stopwatch sw = new Stopwatch();
                                    sw.Start();
                                    int[,] coor;
                                    int maxCCL = getComponents(mask,out coor);
                                    sw.Stop();
                                    UnityEngine.Debug.Log(" the max labeled cc = " + maxCCL);
                                    TimeSpan ts = sw.Elapsed;
                                    UnityEngine.Debug.Log(" le temps d'execution est = " + ts.Hours +":"+ts.Minutes+":"+ts.Seconds+"."+ts.Milliseconds);
                                    nbrloop = 0;
                                }

                            }
                            else
                            {
                                Text.text = "the mask is empty the mask is emptyyyyyyy";
                            }
                            //Text.text = "end of trackableupdate function !!";
                            //Cv2.WaitKey(30);

                        }
                        else
                        {
                            Text.text = "else of !mat.empty()";
                            UnityEngine.Debug.Log("the mat is empty !! ");
                        }
                        
                    }


                }
                //Text.text = "after the if of pixels !!";
            }
            //Text.text = "after if of image!!";
        }
}
/// <summary>
/// Called when app is paused / resumed
/// </summary>
void OnPause(bool paused)
{
    if (paused)
    {
        UnityEngine.Debug.Log("App was paused");
        UnregisterFormat();
    }
    else
    {
        UnityEngine.Debug.Log("App was resumed");
        RegisterFormat();
    }
}
/// <summary>
/// Register the camera pixel format
/// </summary>
void RegisterFormat()
{
    if (CameraDevice.Instance.SetFrameFormat(mPixelFormat, true))
    {
        UnityEngine.Debug.Log("Successfully registered camera pixel format " + mPixelFormat.ToString());
        mFormatRegistered = true;
    }
    else
    {
        UnityEngine.Debug.LogError("Failed to register camera pixel format " + mPixelFormat.ToString());
        mFormatRegistered = false;
    }
}
/// <summary>
/// Unregister the camera pixel format (e.g. call this when app is paused)
/// </summary>
void UnregisterFormat()
{
    UnityEngine.Debug.Log("Unregistering camera pixel format " + mPixelFormat.ToString());
    CameraDevice.Instance.SetFrameFormat(mPixelFormat, false);
    mFormatRegistered = false;
}
    #endregion //PRIVATE_METHODS

    Mat getImgmat(Vuforia.Image img, PIXEL_FORMAT mPixelFormat)
    {
        Mat imgMat = null;
        if (mPixelFormat == PIXEL_FORMAT.RGBA8888)
        {
            Text.text = "before creating of the matrix(cas if) !!";
            imgMat = new Mat(img.Height, img.Width, MatType.CV_8UC4);
            UnityEngine.Debug.Log("\nopencv matrix created (cas if) !! ");
            Text.text = "opencv matrix created (cas if) !!";
        }
        else if (mPixelFormat == PIXEL_FORMAT.RGB888)
        {
            Text.text = "before creating of the matrix (cas else) !!";
            imgMat = new Mat(img.Height, img.Width, MatType.CV_8UC3);
            UnityEngine.Debug.Log("\nopencv matrix created (cas else) !!");
            Text.text = "opencv matrix created (cas else) !!";

        }
        return imgMat;
    }
    Mat getRGBmat(Mat Imgmat)
    { 
        Mat frame_rgb = Imgmat;
        if (mPixelFormat == PIXEL_FORMAT.RGBA8888)
        {    
            frame_rgb = Imgmat.CvtColor(ColorConversionCodes.RGBA2RGB);
            Text.text = "cvt color function case rgba !!";
        }
        return frame_rgb;
    }
    Mat getHSVmat(Mat frame_rgb)
    {
        return frame_rgb.CvtColor(ColorConversionCodes.RGB2HSV);
    }
    Mat getMask(Mat frame_hsv, int[] low_red,int[] high_red)
    {
        return frame_hsv.InRange(InputArray.Create(low_red), InputArray.Create(high_red));
    }
    int[,] getCCL(Mat m)
    {
        int[,] t = new int[m.Height,m.Width];
        List<int[]> e = new List<int[]>();
        int i, j, etiq = 1, topi,topj,lefti,leftj, cri,crj,cli,clj;
        bool b = false;
        int[] z = new int[2];
        int h = m.Height, w = m.Width;

        for(i=0; i<h; i++)
        {
            for(j=0; j<w; j++)
            {
                if(m.GetArray(i,j)[0] != 0)
                {
                    b = false;
                    topi = i - 1; topj = j;
                    lefti = i; leftj = j - 1;
                    cli = i - 1; clj = j - 1;
                    cri = i - 1; crj = j + 1;

                    if(crj<w && cri < h)
                    {
                        if(t[cri, crj] != 0)
                        {
                            t[i, j] = t[cri, crj];
                            b = true;
                        }  
                    }
                    if( b==false && topi<h )
                    {
                        if(t[topi, topj] != 0)
                        {
                            t[i, j] = t[topi, topj];
                            b = true;
                        }
                    }
                    if(cli<h && clj<w )
                    {
                        if(t[cli, clj] != 0)
                        {
                            if (b == false)
                            {
                                t[i, j] = t[cli, clj];
                                b = true;
                            }
                            else if (cri < h && crj < w)
                            {
                                if(t[cli, clj] != t[cri, crj])
                                {
                                    z[0] = t[cli, clj];
                                    z[1] = t[cri, clj];
                                    e.Add(z);
                                    b = true;
                                }   
                            }
                        } 
                    }
                    if(leftj<w )
                    {
                        if(t[lefti, leftj] != 0)
                        {
                            if (t[i, j] != t[lefti, leftj] && t[i, j] != 0)
                            {
                                z[0] = t[i, j];
                                z[1] = t[lefti, leftj];
                                e.Add(z);
                                b = true;
                            }
                            else
                            {
                                t[i, j] = t[lefti, leftj];
                                b = true;
                            }
                        }       
                    }
                    if(b == false)
                    {
                        t[i, j] = etiq;
                        etiq++;
                    }

                }
            }
        }

        // 2nd pass
        
        foreach (int[] el in e)
        {
            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++)
                {
                    if (t[i, j] == el[1]) t[i, j] = el[0];
                }
            }
            for (i = 0; i < h; i++)
            {
                for (j = 0; j < w; j++)
                {
                    if (t[i, j] > el[1]) t[i, j]--;
                }
            }
            foreach (int[] k in e)
            {
                for(int l=0; l<2; l++)
                {
                    if (k[l] > el[1]) k[l]--;
                }
            }
        }
        return t;
    }

    int getMaxCCL(int[,] m,int h, int w)
    {
        int max = 0;
        for (int i = 0; i < h; i++)
        {
            for (int j = 0; j < w; j++)
            {
                if (m[i, j] > max) max = m[i, j];
            }
        }
        return max;
    }

    int getComponents(Mat m, out int[,] coor)
    {
        int etiq = 0, topi = -1,topj = -1 , minj = -1, maxj = -1, bui=-1 , buj=-1, nbz = 0, i =0, j=0;
        coor = new int[34, 6];

        while(i<m.Height)
        {
            j = 0;
            nbz = 0;
            while ( j < m.Width)
            {
                if(m.GetArray(i,j)[0] != 0)
                {
                    bui = i; buj = j;
                    if (topi == -1)
                    {
                        etiq++;
                        topi = i;
                        topj = j;
                        j = m.Width;
                        minj = j;
                        maxj = j;
                    }
                    else
                    {
                        if (j<minj)
                        {
                            minj = j;
                        }else
                        {
                            maxj = j;
                        }
                    }
                }
                else
                {
                    nbz++;
                }
                j++;
            }
            if( nbz == m.Width && topi != -1)
            {
                coor[etiq, 0] = topi;
                coor[etiq, 1] = topj;
                coor[etiq, 2] = minj;
                coor[etiq, 3] = maxj;
                coor[etiq, 4] = bui;
                coor[etiq, 5] = buj;
                topi = -1;
                topj = -1;
                minj = -1;
                maxj = -1;
                bui = -1;
                buj = -1;
            }
            i++;
        }
        return etiq;

    }




}
