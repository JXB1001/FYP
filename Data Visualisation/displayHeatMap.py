import matplotlib.pyplot as plt
import numpy as np
import math
from PIL import Image
import PIL
import math


GlobalMax = 0

def getValue(value, split, number, min):
    value = float(value)
    for i in range(number):
        if( (value >= (min + (split*i))) and (value < (min + (split*(i+1)))) ):
            return i

    return -1

def myFunc(input):
    # return input**0.25

    if(input <= 1):
        return 1

    # return 0

    return abs(input-GlobalMax)**7



count = 0
recordingFile = "b_finalData.csv"

f = open(recordingFile)
inc = 50
scaledInc = 50
indents = 5

xLabels = []
yLabels = []



minmax = [0,1.0,0,1.0,0,1.0]


# for line in f:
#     line = line.strip()
#     data = line.split(',')

#     for i, d in enumerate(data):
#         if(float(d) < minmax[i*2]):
#             # finding the minimum
#             minmax[i*2] = float(d)
#         if(float(d) > minmax[(i*2)+1]):
#             # finding the maximum
#             minmax[(i*2)+1] = float(d)
#     count += 1

# f.close()

# print(count)

accPosition = 512-2

aInc = (minmax[1] - minmax[0])/inc
bInc = (minmax[3] - minmax[2])/inc

matrix = np.zeros((inc, inc))
matrixCount = np.ones((inc, inc))

matrix2 = np.zeros((inc, inc, inc))
matrixCount2 = np.zeros((inc, inc, inc))

f = open(recordingFile)

previousValue = 0

print("Collecting Data...")
count = 0
for line in f:
    if count != 0:
        line = line.strip()
        data = line.split(',')
        flag = False
        # if(float(data[0]) < 0):
        #     temp = 8.0
        #     flag = True
        # else:
        #     temp = float(data[0])

        if(True):
            x = getValue(data[0], aInc, inc, minmax[0])
            y = getValue(data[1], bInc, inc, minmax[2])
            z = getValue(previousValue, 1/inc, inc, 0)
            matrix[x,y] += float(data[accPosition])
            matrixCount[x,y] += 1
            matrix2[x,y,z] += float(data[accPosition])-previousValue
            matrixCount2[x,y,z] += 1
            previousValue = float(data[accPosition])
    count += 1


print("Processing Data...")
for x in range(inc):
    for y in range(inc):
        GlobalMax = max(GlobalMax, matrixCount[x, y])
        for z in range(inc):
            matrix2[x,y,z] += 0.0
            matrixCount2[x,y,z] += 1


# x in follow distance
# y is currentspeed

f.close()

newResult = np.divide(matrix, matrixCount)
# print(matrix)
# print(matrixCount)

result2_1 = np.divide(matrix2, matrixCount2)
# print(matrix2)
# print(matrixCount2)

newmatrix = result2_1[inc-1, :, :]
newMatrixCount = matrixCount2[inc-1,:,:]

myFuncVec = np.vectorize(myFunc)
result1 = matrix
result2 = matrixCount
result3 = newResult
result5 = myFuncVec(matrixCount)

for i in range(scaledInc):
    if(i%indents == 0):
        yLabels.append(str(round(1-(i*aInc*(inc/scaledInc))+minmax[0], 1 )))
    else:
        yLabels.append('')

for i in range(scaledInc):
    if(i%indents == 0):
        xLabels.append(str(round( (i*bInc*(inc/scaledInc))+minmax[2], 1)))
    else:
        xLabels.append("")


# img = Image.fromarray(result5)
# newimg = img.resize((scaledInc, scaledInc), resample=PIL.Image.BICUBIC)
# result4 = np.asarray(newimg)


newmatrix = matrixCount2[inc-1, :, :]
fig, ax = plt.subplots()
im = ax.imshow(result3, cmap='rainbow')

#ax = plt.imshow(matrix, cmap='plasma', interpolation='nearest')
ax.set_xlabel("Current Speed")
ax.set_ylabel("Follow Distance")
ax.set_xticks(np.arange(len(xLabels)))
ax.set_yticks(np.arange(len(yLabels)))
ax.set_xticklabels(xLabels)
ax.set_yticklabels(yLabels)
ax.set_title("Average Acceleration Value")
plt.show()




#np.savetxt("myAccDef.csv", result4, delimiter=",")

# resList = result3.tolist()
# print(result3.shape)

# jsonString = ''
# jsonString += '2\n'
# jsonString += (str(minmax[0]) + '\n')
# jsonString += (str(minmax[1]) + '\n')
# jsonString += (str(minmax[2]) + '\n')
# jsonString += (str(minmax[3]) + '\n')
# jsonString += (str(resList)+'\n')

# mydef = open("myAccData.txt", 'w')
# mydef.write(jsonString)
# mydef.close()

# ##################################################
# ## Writing to second file

# resList2 = result2_1.tolist()

# jsonString = ''
# jsonString += '3\n'
# jsonString += (str(minmax[0]) + '\n')
# jsonString += (str(minmax[1]) + '\n')
# jsonString += (str(minmax[2]) + '\n')
# jsonString += (str(minmax[3]) + '\n')
# jsonString += (str(resList2)+'\n')

# mydef = open("myAccData2.txt", 'w')
# mydef.write(jsonString)
# mydef.close()