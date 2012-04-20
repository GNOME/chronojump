# 
#  This file is part of ChronoJump
# 
#  ChronoJump is free software; you can redistribute it and/or modify
#   it under the terms of the GNU General Public License as published by
#    the Free Software Foundation; either version 2 of the License, or   
#     (at your option) any later version.
#     
#  ChronoJump is distributed in the hope that it will be useful,
#   but WITHOUT ANY WARRANTY; without even the implied warranty of
#    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
#     GNU General Public License for more details.
# 
#  You should have received a copy of the GNU General Public License
#   along with this program; if not, write to the Free Software
#    Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
# 
#   Copyright (C) 2012   Teng Wei Hua <wadedang@gmail.com>
# 
# encoding=utf-8
#
# chronopic port scanning 
# This program will find which port the chronopic used for encoder.
# OS: windows
# History:
#    2012-04-19 beta

import serial
w_baudrate = 115200


def port_scan():
    w_chronopic_port_list = list()
    for i in xrange(256):
        try:
            temp_serial = serial.Serial(i, w_baudrate)
            temp_serial.write('J')
            for j in xrange(50):
                if temp_serial.read() == 'J':
                    w_chronopic_port_list.append(i)
                    break
            temp_serial.close()
        except:
            pass
    return w_chronopic_port_list


# ================
# = Main Problem =
# ================

print port_scan()




