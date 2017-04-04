using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Services.Maps;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Weichen_s_Mastermind
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Game3 : Page
    {
        Geolocator myGeo;
        Geoposition _pos;

        int _numTurns = 10;
        public Game3() // constructor
        {
            this.InitializeComponent();
        }

        
        
        //  need to override onnavigatedto method
        // to continue game or create new.


        private void createBoard(int iTurns)
        {
            #region Variables
            int i, j, iPegs = 4;     //个数
            StackPanel spTurn;
            Ellipse el;
            Grid grd;
            #endregion

            for (i = iTurns - 1; i >= 0; i--)
            {
                #region Create spTurn Stack Panel
                spTurn = new StackPanel();
                spTurn.Name = "spTurn" + i.ToString();
                spTurn.Orientation = Orientation.Horizontal;
                spTurn.Margin = new Thickness(3);                            //(上，右，下，左)
                spTurn.HorizontalAlignment = HorizontalAlignment.Center;
                spAllTurns.Children.Add(spTurn);
                #endregion
                //玩家填充
                #region Add the player pegs
                for (j = 0; j < iPegs; j++)
                {
                    el = new Ellipse();
                    el.Name = "peg_" + i.ToString() + "_" + j.ToString();
                    // h, w, m, fill, stroke
                    el.Height = 30;
                    el.Width = 30;
                    el.Margin = new Thickness(3);
                    el.Fill = new SolidColorBrush(Colors.White);
                    el.Stroke = new SolidColorBrush(Colors.Black);       
                    // += is used to add event handlers to objects
                    // -= is used to take them away
                    el.Tapped += playerPeg_Tapped;
                    spTurn.Children.Add(el);
                }
                #endregion

                //Feedback
                #region Add the feedback pegs
                grd = new Grid();
                grd.Name = "grdFeedback" + i.ToString();
                grd.Height = 30;
                grd.Width = 30;
                grd.Margin = new Thickness(3);
                grd.RowDefinitions.Insert(grd.RowDefinitions.Count, new RowDefinition());
                grd.RowDefinitions.Insert(grd.RowDefinitions.Count, new RowDefinition());
                grd.ColumnDefinitions.Insert(grd.ColumnDefinitions.Count, new ColumnDefinition());
                grd.ColumnDefinitions.Insert(grd.ColumnDefinitions.Count, new ColumnDefinition());
                spTurn.Children.Add(grd);

                for (int r = 0; r < grd.RowDefinitions.Count; r++)
                {
                    for (int c = 0; c < grd.ColumnDefinitions.Count; c++)
                    {
                        el = new Ellipse();
                        el.Name = "FBPeg_" + i.ToString() + "_" +
                                            r.ToString() + "_" +
                                            c.ToString();
                        el.Fill = new SolidColorBrush(Colors.White);
                        el.Stroke = new SolidColorBrush(Colors.Black);
                        el.Height = 11;
                        el.Width = 11;
                        el.Margin = new Thickness(2);
                        el.SetValue(Grid.RowProperty, r);
                        el.SetValue(Grid.ColumnProperty, c);
                        grd.Children.Add(el);

                    }

                }


                #endregion
            }
            //正确答案板块
            #region Create Combination to guess           
            for (j = 0; j < iPegs; j++)
            {
                el = new Ellipse();
                el.Name = "combo_" + j.ToString();
                // h, w, m, fill, stroke
                el.Height = 30;
                el.Width = 30;
                el.Margin = new Thickness(3);
                el.Fill = new SolidColorBrush(Colors.Transparent);
                el.Stroke = new SolidColorBrush(Colors.Black);
                spCombo.Children.Add(el);
            }
            generateCombination();
            cvsCover.Opacity = 0.9;
            #endregion

            //// enable the first turn (last one drawn)
            //spTurn = (StackPanel)spAllTurns.FindName("spTurn" + (iTurns-1).ToString());
            //spTurn.IsTapEnabled = true;

        }

        Ellipse _curr;
        int _turnNumber;
        private void playerPeg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            _curr = (Ellipse)sender;

            if (!(_curr.Name.Contains("peg_" + _turnNumber.ToString())))
            {
                return;
            }


            // make the panel visible so the user can choose
            spChooseColour.Visibility = Visibility.Visible;
            tblTest.Text = _curr.Name;



        }

        private void chooseColourPeg_Tapped(object sender, TappedRoutedEventArgs e)
        {
            // copy the colour of the tapped ellipse (sender) here to the 
            // current player peg ellipse
            Ellipse curr = (Ellipse)sender;

            _curr.Fill = curr.Fill;

            // set the _curr to nothing
            _curr = null;


            spChooseColour.Visibility = Visibility.Collapsed;

        }

        private void btnSubmit_Tapped(object sender, RoutedEventArgs e)
        {
            Ellipse el;
            int iTurns = _numTurns; // added from constructor

            if (btnSubmit.Content.ToString() == "Start Game")
            {
                // put constructor code here
                // read from settings the number of turns.
                // pass it to the createBoard function
                createBoard(iTurns);
                _turnNumber = 0;
                generateCombination();

                btnSubmit.Content = "Submit";
            }
            else // check for submitted move
            {
                // need to check for last turn.
                // if turnNumber = _numTurns - then game us over
                if (_turnNumber == _numTurns)
                    return;
                // peg_0_0
                StackPanel sp = (StackPanel)spAllTurns.FindName("spTurn" + _turnNumber.ToString());
                Ellipse elTest = new Ellipse();
                elTest.Fill = new SolidColorBrush(Colors.Transparent);

                //SolidColorBrush br = (SolidColorBrush)elTest.Fill);
                //Color col = br.Color; 
                // check for complete row
                for (int i = 0; i < 3; i++)
                {
                    el = (Ellipse)sp.FindName("peg_" + _turnNumber.ToString() +
                                              "_" + i.ToString());
                    if (((SolidColorBrush)el.Fill).Color == Colors.Transparent)
                    {
                        tblTest.Text = el.Name + " - Invalid Move";
                        return;
                    }
                }

                // check the combination
                // disable the current turn stack panel
                _turnNumber++;

            } // end if (btnSubmit.Content.ToString() == "Start Game")

        }

        private void generateCombination()
        {
            // use random number generator to generate
            // you do this part :-)
            //           [0,3,1,2]

            // using 8 colours to choose from.
            // declare here for testing

            //string[] myColours = { "Red", "Orange" };
            int i;
          Random x = new Random();

        for( int j = 0; j <= 3; j++)
            {
                i = x.Next(0, 6);
                // add to the combination and get the next colour
                
            }

            // use a loop to pick 4 colours from the available 8
            // add to the combination for the user to guess.

          
            int[] comboToGuess = { 0, 1, 2, 3 };

            Color[] myColours = { Colors.Red, Colors.Green,
                                  Colors.Blue, Colors.Black };   
            Ellipse el;
            // put the colours into the combo ellipses
            for (i = 0; i < 4; i++) // get rid of the 4 later.
            {
                // find each combo ellipse and fill in the colour
                el = (Ellipse)spCombo.FindName("combo_" + i.ToString());
                // change the colour value of the fill object
                ((SolidColorBrush)el.Fill).Color =
                                myColours[comboToGuess[i]];
            }




            // fourish colours (numbers) and set these as the 
            // combination to guess.
            // rules of colours
            // 4 colours in combination
            // may be duplicates
            // max is 2 duplicates
            // set up as an array or four colour variables.
            // if statement to check them
            // create a combination class with methods to check the answer
            // and a method to generate a new combination
            // maybe a user control would be better


        }

        private void svPoint_ViewChanged(object sender, ScrollViewerViewChangedEventArgs e)
        {

        }

        private void elInit_Tapped(object sender, TappedRoutedEventArgs e)
        {
            initialiseGeoLocation();
        }

        private async void initialiseGeoLocation()
        {
            var access = await Geolocator.RequestAccessAsync();
            switch (access)
            {
                case GeolocationAccessStatus.Allowed:
                    tblStatus.Text = "                                 Setting up location services";
                    myGeo = new Geolocator();
                    myGeo.StatusChanged += myGeo_StatusChanged;
                    myGeo.DesiredAccuracy = PositionAccuracy.Default;
                    break;
                case GeolocationAccessStatus.Denied:
                case GeolocationAccessStatus.Unspecified:
                    tblStatus.Text = "                                 Can't access location services";
                    break;
                default:
                    break;

            }
        }

        private async void myGeo_StatusChanged(Geolocator sender, StatusChangedEventArgs args)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal,
                () =>
                {
                   #region Switch(args.Status)
                   switch (args.Status)
                    {
                        case PositionStatus.Ready:
                            tblStatus.Text = "                                 All good";
                            break;
                        case PositionStatus.Initializing:
                            tblStatus.Text = "                                 Initializing";
                            break;
                        case PositionStatus.NoData:
                            tblStatus.Text = "                                 No Data";
                            break;
                        case PositionStatus.Disabled:
                            tblStatus.Text = "                                 Disabled";
                            break;
                        case PositionStatus.NotInitialized:
                            tblStatus.Text = "                                 Not Initializing";
                            break;
                        case PositionStatus.NotAvailable:
                            tblStatus.Text = "                                 Not Available";
                            break;
                        default:
                            break;
                    }
                   #endregion

               });
        }

        private async void elSavePosition_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                _pos = await myGeo.GetGeopositionAsync();
                displayPointDetails();

            }
            catch
            {
                tblStatus.Text = "                                 Problem reading location!";
                    return;
            }

        }

        private async void displayPointDetails()
        {
            StackPanel sp;
            TextBlock tblTime, tblLong, tbLat;
            sp = new StackPanel();
            sp.Margin = new Windows.UI.Xaml.Thickness(2);

            tblTime = new TextBlock();
            BasicGeoposition myLocation = new BasicGeoposition();
            myLocation.Latitude = _pos.Coordinate.Point.Position.Latitude;
            myLocation.Longitude = _pos.Coordinate.Point.Position.Longitude;

            Geopoint pointReverse = new Geopoint(myLocation);
            MapLocationFinderResult result = await MapLocationFinder.FindLocationsAtAsync(pointReverse);
            if (result.Status == MapLocationFinderStatus.Success)
            {
                string name = result.Locations[0].Address.Town;
                tblTime.Text = "Loc: " + name;
            }
            else
            {
                tblTime.Text = "Time: " + _pos.Coordinate.Timestamp.Date.ToString();
            }
            tbLat = new TextBlock();
            tbLat.FontSize = 32;
            tbLat.Text = "Lat: " + _pos.Coordinate.Point.Position.Latitude.ToString();

            tblLong = new TextBlock();
            tblLong.FontSize = 32;
            tblLong.Text = "Long: " + _pos.Coordinate.Point.Position.Longitude.ToString();

            sp.Children.Add(tblTime);
            sp.Children.Add(tbLat);
            sp.Children.Add(tblLong);

            spLocation.Children.Add(sp);
        }

        private void tblStatus_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
