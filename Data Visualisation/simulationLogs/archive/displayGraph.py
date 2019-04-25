import numpy as np
import matplotlib.pyplot as plt

rf = open("C:/Users/JXB1001/Documents/MEng Project/Simulation/simulationLogs/speedLog.txt", "r")
x = []
y = []

for line in rf:
    l = line.split()
    x.append(float(l[0]))
    y.append(float(l[1]))

plt.plot(x, y)
plt.show()
