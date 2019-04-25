import csv
import os

filelocation = "C:/Users/JXB1001/Documents/MEng Project/Simulation/simulationLogs/logs/"

fileList = os.listdir(filelocation)

table = []

for line in fileList:
    tempList = []
    tf = open(filelocation+line, 'r')
    for i in tf:
        tempList.append(i.split(',')[1].strip())
    table.append(tempList)
    tf.close()

tableFile = open("C:/Users/JXB1001/Documents/MEng Project/Simulation/simulationLogs/jsData.js", 'w')
tableFile.write('var ed = ' + str(table) + ';')



    
