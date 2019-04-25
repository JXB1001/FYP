from __future__ import absolute_import, division, print_function

import sys
import os
import pandas as pd
# import numpy as np

import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers

length_of_data = 512

buf = open("C:\\Users\\jsben\\OneDrive\\Documents\\UnityFiles\\buffer.txt")
observations = buf.readline().split(',')
buf.close()
obs = {}

for i, o in enumerate(observations):
    obs[i] = float(o)

idf = pd.DataFrame.from_dict([obs])
idf.pop(length_of_data-1)

checkpoint_path = "C:/Users/jsben/OneDrive/Documents/UnityFiles/steeringTraining/cp.ckpt"
checkpoint_dir = os.path.dirname(checkpoint_path)

model = keras.Sequential([
    keras.layers.Flatten(input_shape=(length_of_data-1,)),
    keras.layers.Dense(length_of_data-1, activation=tf.nn.relu),
    keras.layers.Dense(3, activation=tf.nn.softmax)
])

model.load_weights(checkpoint_path)
predictions = model.predict(idf)
index = 0
tempMax = 0
for i, p in enumerate(predictions[0]):
    if(p > tempMax):
        index = i
        tempMax = p

print(str(index))











# dataset_path = "cleanedData.csv"
# class_names = ['move_down', 'stay', 'move_up']
# column_names = []
# for i in range(818):
#     column_names.append(i)

# raw_dataset = pd.read_csv(dataset_path, names=column_names,
#                       na_values = " ", comment='\t',
#                       sep=",", skipinitialspace=True)

# dataset = raw_dataset.copy()
# dataset.tail()

# train_dataset = dataset.sample(frac=0.9,random_state=0)
# test_dataset = dataset.drop(train_dataset.index)

# train_labels = train_dataset.pop(817)
# test_labels = test_dataset.pop(817)

# predictions = model.predict(test_dataset)
# print(predictions)
