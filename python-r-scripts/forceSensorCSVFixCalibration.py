import pandas as pd
from os import listdir
from os import chdir

chdir("forceSensorCSVFixCalibration_files_tofix")
for filename in listdir():
    #about pandas_read_csv(engine): https://stackoverflow.com/a/47922247
    #df = pd.read_csv(filename, sep=";", decimal=",", engine='python')
    df = pd.read_csv(filename, sep=";", decimal=",")
    for i, row in df.iterrows():
        current = float(df.at[i,'Force(N)'])
        new = ((current -257.13) * 13.4) / 930.59 #obviously, change the values
        df.at[i,'Force(N)'] = new
        #print("row: " + str(i) + "; current: " + str(current) + "; new: " + str(new))
    
    df.to_csv("../forceSensorCSVFixCalibration_files_fixed/" + filename, sep=';', decimal=",", index=False)
