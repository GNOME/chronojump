Guide for Debian (Ubuntu...) systems with python2.7 and 32 bits!!
-----------------------------------------------------------------

1.- Install python-wx:

sudo apt-get install python-wxgtk2.8

2.- Install libstargate

sudo dpkg -i python-libstargate_1.2-1_i386-ubuntu-10.04.deb

3.- Compile libiris for python2.7

cd LibIris/libiris-1.2
sudo python setup.py install


In **** 64 bits **** systems, installing libiris, can make run the chronopic-firmwarecord





---- more stuff

- If needed, there's a there's a python-libstargate package, maybe works

- libiris is also here:
http://www.iearobotics.com/wiki/index.php?title=LibIris
http://www.iearobotics.com/wiki/images/2/21/Libiris-1.2.zip
