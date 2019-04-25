import random

oldfile = open("b_finalData.csv")
newfile = open("cleanedData3.csv", 'w')
newnewfile = open("cleanedData4.csv", 'w')


buffer = []
givenClass = 0
changeFlag = True
classCount = [0,0,0]
listOfIndexes = []
count = 0
for line in oldfile:
    data = line.split(',')
    for d in data:
        d = d.strip()
    if(float(data[3]) == 0) and changeFlag:
        givenClass = 0
        changeFlag = False
    else:
        givenClass = 1
    
    if(float(data[3]) != 0): 
        changeFlag = True

    if(buffer):
        if(givenClass == 0):
            value = float(data[-1])
            if(value < 0):
                givenClass = 0
            elif(value > 0):
                givenClass = 2
            else:
                print("error")

        classCount[givenClass] += 1
        if(givenClass == 1):
            listOfIndexes.append(count)
        buffer[-1] = str(givenClass)
        newline = ','.join(buffer)
        newfile.write(newline + "\n")

    buffer = data
    count += 1
    if(count%1000 == 0):
        print(count)

oldfile.close()
newfile.close()

numberToFind = max(classCount[0], classCount[2])*3

indexesToFind = []

while(len(indexesToFind) < numberToFind):
    indexesToFind.append(listOfIndexes.pop(random.randint(0, len(listOfIndexes))))


newfile = open("cleanedData3.csv")
newnewfile = open("cleanedData4.csv", 'w')

print(classCount)

count = 0
for line in newfile:
    appendline = False
    if(count in indexesToFind):
        appendline = True
    
    if(appendline == False):
        data = line.split(',')
        data[-1] = data[-1].strip()
        if(int(data[-1]) != 1):
            appendline = True
    
    if(appendline):
        newnewfile.write(line)

    count += 1
    if(count%1000 == 0):
        print(count)

newfile.close()
newnewfile.close()






