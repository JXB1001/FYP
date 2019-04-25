from __future__ import absolute_import, division, print_function

import sys
import os
import pandas as pd
# import numpy as np

import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers

length_of_data = 512

def build_model():
  model = keras.Sequential([
    layers.Dense(length_of_data-1, activation=tf.nn.relu, input_shape=[length_of_data-1]),
    layers.Dense(length_of_data-1, activation=tf.nn.relu),
    layers.Dense(1)
  ])
  return model

buf = open("C:\\Users\\jsben\\OneDrive\\Documents\\UnityFiles\\buffer.txt")
observations = buf.readline().split(',')
buf.close()
obs = {}

for i, o in enumerate(observations):
    obs[i] = float(o)

idf = pd.DataFrame.from_dict([obs])
idf.pop(length_of_data-2)

checkpoint_path = "C:/Users/jsben/OneDrive/Documents/UnityFiles/modelSave1/cp-0200.ckpt"
checkpoint_dir = os.path.dirname(checkpoint_path)

model = build_model()
model.load_weights(checkpoint_path)
predictions = model.predict(idf)
print(predictions[0][0])