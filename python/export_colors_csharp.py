import matplotlib.pyplot as plt
import numpy as np
import os

scriptPath = os.path.abspath(__file__)
colorPath = os.path.join(os.path.dirname(os.path.dirname(scriptPath)), 'colors')

if not os.path.exists(colorPath):
    os.makedirs(colorPath)

cmapName = 'plasma'
nSamples = 50

cmap = plt.get_cmap(cmapName)
colors = cmap(np.linspace(0.0, 1.0, nSamples))
colors = (colors * 255).astype(int)

with open(os.path.join(colorPath, cmapName+'.txt'), 'w') as f:
    f.write("Color[] colorRange = {\n")
    for color in colors:
        f.write(f"    Color.FromArgb(alpha, {color[0]}, {color[1]}, {color[2]}),\n")
    f.write("};")
