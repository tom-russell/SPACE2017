import os
import matplotlib.pyplot as plt
import numpy as np
    
batchPath = "D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE2017\Results\qsettingsGen"
#batchPath = r"C:\Users\tom_j\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE2017\space\Results\qsettingsGen"

dirs = [d for d in os.listdir(batchPath) if os.path.isdir(os.path.join(batchPath, d))]

quorumList = [0,2,5,8,10,12,15,18,20]
presentQuorums = []
accuracyData = [None] * 21
timeData = [None] * 21


for experimentDir in dirs:
    # gathering emigration accuracy data
    dataFilePath = os.path.join(batchPath, experimentDir, "endSimData.txt")
    experimentString = experimentDir.split('_')
    
    if os.path.getsize(dataFilePath) == 0:
        continue
    
    file = open(dataFilePath, 'r')
    nestAllegiances = file.readline().split(',')
    
    quorum = int(experimentString[1][1:])
    if quorum not in presentQuorums:
        presentQuorums.append(quorum)
    
    repeat = int(experimentString[2][1:])
    if repeat is 0:
        accuracyData[quorum] = []   
    
    propBestNest = float(nestAllegiances[2]) / 200#(float(nestAllegiances[1]) + float(nestAllegiances[2]))

    accuracyData[quorum].append(propBestNest)
    
    file.close()
    
    # gathering execution time data
    executionFilePath = os.path.join(batchPath, experimentDir, "execution.txt")
    file = open(executionFilePath, 'r')
    simulationTime = file.readlines()[3][21:].strip('\n').split(":")
    simTimeSecs = int(simulationTime[0]) * 3600 + int(simulationTime[1]) * 60 + int(simulationTime[2])
    
    if repeat is 0:
        timeData[quorum] = []
        
    timeData[quorum].append(simTimeSecs)
    file.close()
    
# averaging values and determining standard deviations
accuracyAverageValues = []
accuracyStdValues = []
timeAverageValues = []
timeStdValues = []

presentQuorums = sorted(presentQuorums)

for quorum in quorumList:
    print(accuracyData[quorum])

for quorum in presentQuorums:
    #print(accuracyData[quorum])
    
    accuracyRepeats = accuracyData[quorum]
    average = sum(accuracyRepeats) / len(accuracyRepeats)
    accuracyAverageValues.append(average)
    std = np.std(accuracyRepeats)
    accuracyStdValues.append(std)
    
    timeRepeats = timeData[quorum]
    average = sum(timeRepeats) / len(timeRepeats)
    timeAverageValues.append(average)
    std = np.std(timeRepeats)
    timeStdValues.append(std)
   
#print(presentQuorums)
#print(accuracyData)
#print(accuracyAverageValues)
# plotting the accuracy graph   
plt.subplot(2, 1, 1)
plt.title("Accuracy vs Quorum")
plt.plot(presentQuorums, accuracyAverageValues, marker = 'o', color='b')
plt.errorbar(presentQuorums, accuracyAverageValues, yerr=accuracyStdValues, linestyle='None', color='r', capsize=5)
plt.ylabel('Accuracy')
plt.xlabel('Quorum Threshold')
#plt.axis([0,20,0.7,1.0])
#plt.axis([0, 20, 0.8, 1.0])

# plotting the speed graph
plt.subplot(2, 1, 2)
plt.title("Speed vs Quorum")
plt.plot(presentQuorums, timeAverageValues, marker = 'o', color='b')
plt.errorbar(presentQuorums, timeAverageValues, yerr=timeStdValues, linestyle='None', color='r', capsize=5)
plt.ylabel('Speed')
plt.xlabel('Quorum Threshold')

plt.show()

