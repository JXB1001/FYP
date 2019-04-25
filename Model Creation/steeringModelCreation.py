from __future__ import absolute_import, division, print_function

import os
import sys
import pathlib
import matplotlib.pyplot as plt
import pandas as pd
import seaborn as sns
import datetime as dt
import numpy as np

import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers

from tensorflow.python.saved_model import builder as saved_model_builder
from tensorflow.python.saved_model import signature_constants
from tensorflow.python.saved_model import signature_def_utils
from tensorflow.python.saved_model import tag_constants
from tensorflow.python.saved_model import utils
from tensorflow.python.util import compat

class_names = ['move_down', 'stay', 'move_up']

length_of_data = 512

model = keras.Sequential([
    keras.layers.Flatten(input_shape=(length_of_data-1,)),
    keras.layers.Dense(length_of_data-1, activation=tf.nn.relu),
    keras.layers.Dense(3, activation=tf.nn.softmax)
])

model.compile(optimizer='adam', 
              loss='sparse_categorical_crossentropy',
              metrics=['accuracy'])

dataset_path = "cleanedData4.csv"

checkpoint_path = "steeringTraining/cp.ckpt"
checkpoint_dir = os.path.dirname(checkpoint_path)

# Create checkpoint callback
cp_callback = tf.keras.callbacks.ModelCheckpoint(checkpoint_path,
                                                 save_weights_only=True,
                                                 verbose=1)

# Create Dataset

column_names = []
for i in range(length_of_data):
    column_names.append(i)

raw_dataset = pd.read_csv(dataset_path, names=column_names,
                      na_values = " ", comment='\t',
                      sep=",", skipinitialspace=True)

dataset = raw_dataset.copy()
dataset.tail()

train_dataset = dataset.sample(frac=0.9,random_state=0)
test_dataset = dataset.drop(train_dataset.index)

train_labels = train_dataset.pop(length_of_data-1)
test_labels = test_dataset.pop(length_of_data-1)

model.fit(train_dataset, train_labels, epochs=20, callbacks=[cp_callback])

test_loss, test_acc = model.evaluate(test_dataset, test_labels)
print('Test accuracy:' + str(test_acc))

refLabels = []
for l in test_labels:
    refLabels.append(l)

collectedData = {}
collectedData2 = {}
for i in range(3):
    collectedData[i] = {}
    collectedData2[i] = {}
    for j in range(3):
        collectedData[i][j] = 0
        collectedData2[i][j] = 0

labels = []
predictedClasses = []
print("\nPredictions:")
predictions = model.predict(test_dataset)
for i, p in enumerate(predictions):
    string = ""
    string += str(p) + " : "
    tempMax = 0
    tempIndex = 0
    for index, e in enumerate(p):
        if(e > tempMax):
            tempMax = e
            tempIndex = index

    if(p[1] > 0.1):
        collectedData2[refLabels[i]][1] += 1
    else:
        collectedData2[refLabels[i]][tempIndex] += 1

    predictedClasses.append(tempIndex)
    labels.append(refLabels[i])
    string += str(tempIndex) + ' - '
    string += str(refLabels[i])
    collectedData[refLabels[i]][tempIndex] += 1



    print(string)

fig, ax = plt.subplots()
width = 0.25
colours = ['#1E8000', '#2DBF00', '#8DFF71']

sums = []
for i in range(3):
    sums.append(collectedData[0][i]+collectedData[1][i]+collectedData[2][i])

for i in range(3):
    for j in range(3):
        collectedData[j][i]/=sums[i]
        collectedData[j][i] *= 100

d0 = np.array([collectedData[0][0], collectedData[0][1], collectedData[0][2]])
d1 = np.array([collectedData[1][0], collectedData[1][1], collectedData[1][2]])
d2 = np.array([collectedData[2][0], collectedData[2][1], collectedData[2][2]])
positions = np.array([0, 1, 2])

print(d0)
print(d1)
print(d2)

p0 = ax.bar(positions+width*0,d0,width, color=colours[0], bottom=0)
p1 = ax.bar(positions+width*1,d1,width, color=colours[1], bottom=0)  
p2 = ax.bar(positions+width*2,d2,width, color=colours[2], bottom=0)

ax.set_xticks(positions + width)
ax.set_xticklabels(('move down lane', 'stay in lane', 'move up lane'))
ax.legend((p0[0], p1[0], p2[0]), ('move down lane', 'stay in lane', 'move up lane'), title='Predicted Classification')
ax.set_title('Predicted Classification against Actual Classification')
plt.xlabel('Actual Classification')
plt.ylabel('Percentage (%)')

plt.show()






# fig2, ax2 = plt.subplots()

# sums = []
# for i in range(3):
#     sums.append(collectedData2[0][i]+collectedData2[1][i]+collectedData2[2][i])

# for i in range(3):
#     for j in range(3):
#         collectedData2[j][i]/=sums[i]

# d0 = np.array([collectedData2[0][0], collectedData2[0][1], collectedData2[0][2]])
# d1 = np.array([collectedData2[1][0], collectedData2[1][1], collectedData2[1][2]])
# d2 = np.array([collectedData2[2][0], collectedData2[2][1], collectedData2[2][2]])
# positions = np.array([0, 1, 2])

# p0 = ax2.bar(positions+width*0,d0,width, color=colours[0], bottom=0)
# p1 = ax2.bar(positions+width*1,d1,width, color=colours[1], bottom=0)  
# p2 = ax2.bar(positions+width*2,d2,width, color=colours[2], bottom=0)

# ax2.set_xticks(positions + width)
# ax2.set_xticklabels(('move down lane', 'stay in lane', 'move up lane'))
# ax2.legend((p0[0], p1[0], p2[0]), ('move down lane', 'stay in lane', 'move up lane'), title='Predicted Classification')
# ax2.set_title('Predicted Classification against Actual Classification')
# plt.xlabel('Actual Classification')
# plt.ylabel('Percentage (%)')

# plt.show()


