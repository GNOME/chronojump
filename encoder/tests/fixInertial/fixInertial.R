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
#   Copyright (C) 2023  	Xavier de Blas <xaviblas@gmail.com>


#config params
#originalDir = "/home/xavier/chronojump_YOYO_JSB_2023-03-31_08-50-18/encoder/5/data/signal"
originalDir = "/home/xavier/chronojump_Protocolo\ YoYo\ JSB_2023-03-29_18-30-31/encoder/2/data/signal/"
convertedDir = "/home/xavier/informatica/progs_meus/chronojump/encoder/tests/fixInertial/convertedDir"
graph = TRUE


source ("util.R")
doProcess (originalDir, convertedDir, graph)
