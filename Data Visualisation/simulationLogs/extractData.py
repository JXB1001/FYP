import csv
import os

filelocation = "C:/Users/jsben/OneDrive/Documents/UnityFiles/Logs/"

fileList = os.listdir(filelocation)

table = []

maxFrame = -1

for index, line in enumerate(fileList):
    tempList = []
    tf = open(filelocation+line, 'r')
    count = 0
    for i in tf:
        tempDic = {}
        newData = i.strip().split(',')
        tempDic['number'] = index
        tempDic['frame'] = float(newData[0])
        tempDic['position'] = float(newData[1])
        tempDic['lane'] = float(newData[2])
        tempDic['speed'] = float(newData[3])

        if( float(newData[0]) > maxFrame ):
            maxFrame = float(newData[0])
        tempList.append(tempDic)
        
    table += tempList
    tf.close()

newList = sorted(table, key=lambda k: k['frame'])

tableFile = open("C:/Users/jsben/OneDrive/Documents/UnityFiles/simulationLogs/jsData.js", 'w')
tableFile.write('var ed = ' + str(newList) + ';\n')
tableFile.write('var numOfBots = ' + str(len(fileList)) + ';\n')
tableFile.write('var lengthOfData = ' + str(maxFrame/100) + ';\n')




    
