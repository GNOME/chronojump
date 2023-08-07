/*
  function to draw a cartesian coordinate system and plot whatever data you want
  just pass x and y and the graph will be drawn
  huge arguement list
  &d name of your display object
  x = x data point
  y = y datapont
  gx = x graph location (lower left)
  gy = y graph location (lower left)
  w = width of graph
  h = height of graph
  xlo = lower bound of x axis
  xhi = upper bound of x asis
  xinc = division of x axis (distance not count)
  ylo = lower bound of y axis
  yhi = upper bound of y asis
  yinc = division of y axis (distance not count)
  title = title of graph
  xlabel = x axis label
  ylabel = y axis label
  gcolor = graph line colors
  acolor = axis line colors
  pcolor = color of your plotted data
  tcolor = text color
  bcolor = background color
  &redraw = flag to redraw graph on fist call only
*/

void Graph(ILI9341_t3 & d, double x, double y, double gx, double gy, double w, double h, double xlo, double xhi, double xinc, double ylo, double yhi, double yinc, String title, String xlabel, String ylabel, unsigned int gcolor, unsigned int acolor, unsigned int pcolor, unsigned int tcolor, unsigned int bcolor, boolean & startOver)
{
  //double ydiv, xdiv;
  // initialize old x and old y in order to draw the first point of the graph
  // but save the transformed value
  // note my transform funcition is the same as the map function, except the map uses long and we need doubles
  //static double ox = (x - xlo) * ( w) / (xhi - xlo) + gx;
  //static double oy = (y - ylo) * (gy - h - gy) / (yhi - ylo) + gy;
  //double temp;
  //int rot, newrot;

  //Mapping values to coordinates
  x =  (x - xlo) * ( w) / (xhi - xlo) + gx;
  y =  (y - ylo) * (gy - h - gy) / (yhi - ylo) + gy;

  if (startOver == true)
  {

    startOver = false;
    //In startOver, a point is plotted at the most left point
    ox = x;
    oy = y;
  }

  //graph drawn now plot the data
  // the entire plotting code are these few lines...
  // recall that ox and oy are initialized as static above
  //Drawing 3 lines slows the drawing and erasing
  d.drawLine(ox, oy, x, y, pcolor);
  //  d.drawLine(ox, oy + 1, x, y + 1, pcolor);
  //  d.drawLine(ox, oy - 1, x, y - 1, pcolor);
  ox = x;
  oy = y;

}
void redrawAxes(ILI9341_t3 & d, double gx, double gy, double w, double h, double xlo, double xhi, double ylo, double yhi, double yinc, String title, String xlabel, String ylabel, unsigned int gcolor, unsigned int acolor, unsigned int pcolor, unsigned int tcolor, unsigned int bcolor, unsigned int goalColor, boolean resize) {
  redrawAxes(d, gx, gy, w, h, xlo, xhi, ylo, yhi, yinc, title, xlabel, ylabel, gcolor, acolor, pcolor, tcolor, bcolor, goalColor, resize, 0);
}
void redrawAxes(ILI9341_t3 & d, double gx, double gy, double w, double h, double xlo, double xhi, double ylo, double yhi, double yinc, String title, String xlabel, String ylabel, unsigned int gcolor, unsigned int acolor, unsigned int pcolor, unsigned int tcolor, unsigned int bcolor, unsigned int goalColor, boolean resize, int dec)
{
  //double ydiv, xdiv;
  // initialize old x and old y in order to draw the first point of the graph
  // but save the transformed value
  // note my transform funcition is the same as the map function, except the map uses long and we need doubles
  //static double ox = (x - xlo) * ( w) / (xhi - xlo) + gx;
  //static double oy = (y - ylo) * (- h) / (yhi - ylo) + gy;
  double yAxis;
  //double xAxis;

  //Deleting goalForce line
  if (capturingPreSteadiness || capturingSteadiness)
  {
    yAxis =  (forceGoal - ylo) * (-h) / (yhi - ylo) + gy;
    d.drawLine(gx, yAxis, gx + w, yAxis, BLACK);
  }

  if (resize){
    tft.fillRect(0, 0, gx, gy+4, BLACK);
    //Vertical line
    //d.drawLine(gx, gy, gx, gy - h, acolor);
  }

  d.setTextSize(1);
  d.setTextColor(tcolor, bcolor);

  // draw y scale
  for (double i = ylo; i <= yhi; i += yinc)
  {

    // compute the transform
    yAxis =  (i - ylo) * (-h) / (yhi - ylo) + gy;

    //d.drawLine(gx, yAxis, gx + w, yAxis, acolor);
    //If the scale has changed the numbers must be redrawn
    if (resize)
    {
      if (dec == 0) printTftValue(i, gx - 6, yAxis - 3, 1, dec);
      else if (dec > 0) printTftValue(i, gx - 6 - 5 * (dec + 1), yAxis - 3, 1, dec);
    }
  }

  //  xAxis =  (-xlo) * ( w) / (xhi - xlo) + gx;
  //  d.drawLine(gx, gy, gx, gy - h, acolor);

  //now draw the labels

  d.setTextSize(1);
  d.setTextColor(acolor, bcolor);
  d.setCursor(gx , gy + 20);
  d.println(xlabel);

  d.setTextSize(1);
  d.setTextColor(acolor, bcolor);
  d.setCursor(gx - 30, gy - h - 10);
  d.println(ylabel);

  d.setTextSize(2);
  d.setTextColor(tcolor, bcolor);
  d.setCursor(gx , gy - h - 30);
  d.println(title);

  if (capturingPreSteadiness || capturingSteadiness)
  {
    yAxis =  (forceGoal - ylo) * (-h) / (yhi - ylo) + gy;
    d.drawLine(gx, yAxis, gx + w, yAxis, goalColor);
  }
}


void barPlot (float gx, float gy, float w, float h, float yhi, int numBars, int currentIndex, float abRatio, unsigned int color)
{
  /*
     currentIndex is the las updated slot of the array
     a is the separation between bars
     b is the width of the bar
     System of 2 equations:
     abRatio = a/b
     width = n * (a+b) + a
  */
  //Solution of the system of 2 equations
  float b = w / (numBars + abRatio + abRatio * numBars);
  float a = b * abRatio;
  float localX = w - b;
  float barValue = 0;
  float barPixHeight = 0;

  //
  //  for(int i = currentIndex + 1; i <= currentIndex + 10; i++)
  //  {
  //    Serial.print(i % 10);
  //    Serial.print("\t");
  //  }
  //
  //  Serial.println();
  //
  //  for(int i = currentIndex + 1; i <= currentIndex + 10; i++)
  //  {
  //    Serial.print(bars[ i % 10]);
  //    Serial.print("\t");
  //  }
  //
  //
  //  Serial.println();

  //the first bar to plot corresponds to the last updated slot of the array

  //Deleting the previous bars (The older bar are not in the buffer)
  for (int i = 1; i < 10; i++)
  {
    localX -= a;
    barValue = bars[ (currentIndex - i + 10) % 10];
    barPixHeight =  barValue * h / yhi;
    tft.fillRect(gx + localX, gy - barPixHeight , b, barPixHeight, BLACK);
    localX -= b;
  }

  //Deleting the most left Bar
  localX -= a;
  tft.fillRect(gx + localX, gy - h , b, h, BLACK);
  localX = w - b;

  for (int i = 0; i < 10; i++)
  {
    localX -= a;
    barValue = bars[ (currentIndex - i + 10) % 10];
    barPixHeight =  barValue * h / yhi;
    //Serial.println(String(gx+localX) + "," + String(gy) + "\t" + String(b) + "," + String(bars[ (i + 10 - numBars) % 10]));
    if (i == 0) {
      tft.fillRect(gx + localX, gy - barPixHeight , b, barPixHeight, RED);
    } else {
      tft.fillRect(gx + localX, gy - barPixHeight , b, barPixHeight, BLUE);
    }
    localX -= b;
  }

  //  Serial.println("-----");
}
