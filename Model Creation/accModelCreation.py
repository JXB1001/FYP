from __future__ import absolute_import, division, print_function

import os
import pathlib
import matplotlib.pyplot as plt
import pandas as pd
import seaborn as sns
import datetime as dt

import tensorflow as tf
from tensorflow import keras
from tensorflow.keras import layers

from tensorflow.python.saved_model import builder as saved_model_builder
from tensorflow.python.saved_model import signature_constants
from tensorflow.python.saved_model import signature_def_utils
from tensorflow.python.saved_model import tag_constants
from tensorflow.python.saved_model import utils
from tensorflow.python.util import compat

EPOCHS_CONST = 200
length_of_data = 512

class PrintDot(keras.callbacks.Callback):
  def on_epoch_end(self, epoch, logs):
    if((epoch % 10 == 0)and(epoch > 0)): print("  " + str(dt.datetime.now()) + "  Epoch:"+str(epoch))
    print('.', end='')

def build_model():
  model = keras.Sequential([
    layers.Dense(length_of_data, activation=tf.nn.relu, input_shape=[len(train_dataset.keys())]),
    layers.Dense(length_of_data, activation=tf.nn.relu),
    layers.Dense(1)
  ])

  optimizer = tf.keras.optimizers.RMSprop(0.001)

  model.compile(loss='mean_squared_error',
                optimizer=optimizer,
                metrics=['mean_absolute_error', 'mean_squared_error'])
  return model

def plot_history(history):
  hist = pd.DataFrame(history.history)
  hist['epoch'] = history.epoch
  
  plt.figure()
  plt.xlabel('Epoch')
  plt.ylabel('Mean Abs Error [Acceleration Value]')
  plt.plot(hist['epoch'], hist['mean_absolute_error'],
           label='Train Error')
  plt.plot(hist['epoch'], hist['val_mean_absolute_error'],
           label = 'Val Error')
  #plt.ylim([0,5])
  plt.legend()
  
  plt.figure()
  plt.xlabel('Epoch')
  plt.ylabel('Mean Square Error [$Acceleration Value^2$]')
  plt.plot(hist['epoch'], hist['mean_squared_error'],
           label='Train Error')
  plt.plot(hist['epoch'], hist['val_mean_squared_error'],
           label = 'Val Error')
  #plt.ylim([0,20])
  plt.legend()
  plt.show()


print(tf.__version__)

dataset_path = "cleanedData3.csv"

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

print("########################################################")
sns.pairplot(train_dataset[[0,1,length_of_data-2]], diag_kind="kde")
plt.show()
print("########################################################")

train_labels = train_dataset.pop(length_of_data-2)
test_labels = test_dataset.pop(length_of_data-2)

example_dataset = train_dataset.sample()
example_output = train_labels.sample()

checkpoint_path = "modelSave1/cp-{epoch:04d}.ckpt"
checkpoint_dir = os.path.dirname(checkpoint_path)

cp_callback = tf.keras.callbacks.ModelCheckpoint(
    checkpoint_path, verbose=1, save_weights_only=True,
    # Save weights, every 5-epochs.
    period=100)


model = build_model()
model.summary()

example_batch = train_dataset[:10]
example_result = model.predict(example_batch)
print(example_result)

EPOCHS = EPOCHS_CONST

history = model.fit(
  train_dataset, train_labels,
  epochs=EPOCHS, validation_split = 0.2, verbose=0,
  callbacks=[PrintDot()])

model.save_weights(checkpoint_path.format(epoch=EPOCHS_CONST))

hist = pd.DataFrame(history.history)
hist['epoch'] = history.epoch
hist.tail()

plot_history(history)

loss, mae, mse = model.evaluate(test_dataset, test_labels, verbose=0)

print("\nTesting set Mean Abs Error: {:5.2f} Acceleration Value".format(mae))

test_predictions = model.predict(test_dataset).flatten()

plt.scatter(test_labels, test_predictions)
plt.xlabel('True Values [Acceleration Value]')
plt.ylabel('Predictions [Acceleration Value]')
plt.axis('equal')
plt.axis('square')
plt.xlim([0,plt.xlim()[1]])
plt.ylim([0,plt.ylim()[1]])
_ = plt.plot([-100, 100], [-100, 100])
plt.show()

error = test_predictions - test_labels
plt.hist(error, bins = 25)
plt.xlabel("Prediction Error [Acceleration Value]")
_ = plt.ylabel("Count")
plt.show()


example_batch = train_dataset[:10]
example_result = model.predict(example_batch)
print(example_result)

# print("Attempt 1")

saver = tf.train.Saver()
sess = tf.Session()
sess.run(tf.global_variables_initializer())
# saver.save(sess, 'my_test_model')

# Saving the Model
print("Saving the model")


# # example_dataset = tf.convert_to_tensor(example_dataset)
# # example_output = tf.convert_to_tensor(example_output)

# # classification_inputs = utils.build_tensor_info(example_dataset)
# # classification_outputs_classes = utils.build_tensor_info(tf.placeholder(tf.string, name='tf_example'))
# # classification_outputs_scores = utils.build_tensor_info(example_output)

# # classification_signature = signature_def_utils.build_signature_def(
# #   inputs={signature_constants.CLASSIFY_INPUTS: classification_inputs},
# #   outputs={
# #       signature_constants.CLASSIFY_OUTPUT_CLASSES:
# #           classification_outputs_classes,
# #       signature_constants.CLASSIFY_OUTPUT_SCORES:
# #           classification_outputs_scores
# #   },
# #   method_name=signature_constants.CLASSIFY_METHOD_NAME)

# # tensor_info_x = utils.build_tensor_info(example_dataset)
# # tensor_info_y = utils.build_tensor_info(example_output)

# # prediction_signature = signature_def_utils.build_signature_def(
# #     inputs={'sensors': tensor_info_x},
# #     outputs={'accvalue': tensor_info_y},
# #     method_name=signature_constants.PREDICT_METHOD_NAME)

# # # legacy_init_op = tf.group(tf.tables_initializer(), name='legacy_init_op')

# # export_path = "nn_6"
# # print('Exporting trained model to ' + export_path)
# # builder = tf.saved_model.builder.SavedModelBuilder(export_path)
# # builder.add_meta_graph_and_variables(
# #     sess, [tag_constants.SERVING],
# #     signature_def_map={
# #         'predict_acc':
# #             prediction_signature,
# #         signature_constants.DEFAULT_SERVING_SIGNATURE_DEF_KEY:
# #             classification_signature,
# #     },
# #     main_op=tf.tables_initializer())
# # builder.save()


# builder = tf.saved_model.builder.SavedModelBuilder(export_path)
# builder.add_meta_graph_and_variables(
#       sess, [tf.saved_model.tag_constants.SERVING],
#       signature_def_map={},
#       )
# builder.save()

