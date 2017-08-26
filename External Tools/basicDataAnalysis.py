import os
import sys
import matplotlib.pyplot as plt
import numpy as np
       
class DataExtractor:

    # grouped data
    # quorum
    
    # ungrouped data

    def __init__(self, path):
        self.path = batchPath
        self.dirs = [d for d in os.listdir(self.path) if os.path.isdir(os.path.join(self.path, d))]
        
        # These contain all data types/quorums and the repeats are stored in arrays
        self.dataRtrFalse = dict() 
        self.dataRtrTrue = dict()
        
        # These are the repeats calculated as means & standard deviations to be plotted
        self.plotDictRtrTrue = dict() 
        self.plotDictRtrFalse = dict()
        
        # Extract the data and calucate the plottable values
        self.extractAllData()
        self.calculateMeanAndStdev(self.dataRtrFalse, self.plotDictRtrFalse)
        self.calculateMeanAndStdev(self.dataRtrTrue, self.plotDictRtrTrue)
        
    
    def extractAllData(self):
        
        
        # gathering data from output data directories
        for experimentDir in self.dirs:
            dataFilePath = os.path.join(self.path, experimentDir, "endSimData.txt")
            
            # If the file 
            if os.path.getsize(dataFilePath) == 0:
                continue
            
            file = open(dataFilePath, 'r')
            dataLines = file.readlines()
            
            if dataLines[-1].split(": ")[-1] == str(7200):
                continue
            
            # Add each of the parameters from the experiment name (e.g. quorum=20, RTR=false)
            experimentString = experimentDir.split('_')
            quorum = int(experimentString[1][-2:])
            rtr = True if experimentString[3][-4:] == 'true' else False
            dataDict = self.dataRtrTrue if rtr is True else self.dataRtrFalse
            
            for i in range(1, len(dataLines)):
                dataLine = dataLines[i].split(': ')
                dataName = dataLine[0]
                dataValue = float(dataLine[1][:-1])

                self.AddData(dataDict, dataName, quorum, dataValue)

            implementationTime = dataDict['End of Emigration Time'][quorum][-1] - dataDict['First Carry Time'][quorum][-1]
            self.AddData(dataDict, 'Implementation Time', quorum, implementationTime)
            
            nestAllegiances = list(map(int, dataLines[0][:-1].split(',')))
            absoluteEmigrationCohesion = float(nestAllegiances[bestNest]) / 200
            relativeEmigrationCohesion = float(nestAllegiances[bestNest]) / (float(nestAllegiances[worstNest]) + float(nestAllegiances[bestNest]))
            self.AddData(dataDict, 'Absolute Cohesion', quorum, absoluteEmigrationCohesion)
            self.AddData(dataDict, 'Relative Cohesion', quorum, relativeEmigrationCohesion)
            
            accuracy = 1 if nestAllegiances[bestNest] >  nestAllegiances[worstNest] else 0
            self.AddData(dataDict, 'Colony Accuracy', quorum, accuracy)
    
    def calculateMeanAndStdev(self, dataDict, meanStdevDict):
        for dataType in dataDict:
            if dataType == 'Nest Allegiances':
                continue
                
            for quorum in dataDict[dataType]:
                repeats = dataDict[dataType][quorum]

                mean = sum(repeats) / len(repeats)
                self.AddData(meanStdevDict, dataType, quorum, mean)
                std = np.std(repeats)
                self.AddData(meanStdevDict, dataType, quorum, std)

    @staticmethod
    def AddData(dictionary, dataName, quorum, value):
        if dataName not in dictionary:
            dictionary[dataName] = dict()
            
        if quorum not in dictionary[dataName]:
            dictionary[dataName][quorum] = []
            
        dictionary[dataName][quorum].append(value)
    
    def print_data(self, rtr):
        dataDict = self.dataRtrTrue if rtr is True else self.dataRtrFalse
        
        for dataName in dataDict:
            for quorum in dataDict[dataName]:
                print("data: %s/ q: %s/ repeats: %s" % (dataName, quorum, dataDict[dataName][quorum]))
                
                
class DataPlotter:

    def __init__(self, rtrTrue, rtrFalse):
        self.rtrTrue = rtrTrue
        self.rtrFalse = rtrFalse
        self.create_plots([self.rtrFalse, self.rtrTrue], ['r', 'b'])
        
    def create_plots(self, datas, colours):
    
        for i in range(0, len(datas)):
            plt.subplot(2, 3, 1)
            self.plotVsQuorum(datas[i]['Absolute Cohesion'], 'Cohesion', 'Quorum', colours[i])
            plt.subplot(2, 3, 2)
            self.plotVsQuorum(datas[i]['Discovery Time'], 'Discovery Time', 'Quorum', colours[i])
            plt.subplot(2, 3, 3)
            self.plotVsQuorum(datas[i]['First Carry Time'], 'Decision Time', 'Quorum', colours[i])
            plt.subplot(2, 3, 4)
            self.plotVsQuorum(datas[i]['Implementation Time'], 'Implementation Time', 'Quorum', colours[i])
            plt.subplot(2, 3, 5)
            self.plotVsQuorum(datas[i]['End of Emigration Time'], 'Speed', 'Quorum', colours[i])
            plt.subplot(2, 3, 6)
            self.plotVsQuorum(datas[i]['Colony Accuracy'], 'Accuracy', 'Quorum', colours[i])
        
        plt.show()
        
    def plotVsQuorum(self, data, ylabel, xlabel, colour):
        xData = []
        meanData = []
        stdevData = []
        for quorum, meanStdev in data.items():
            xData.append(quorum)
            meanData.append(meanStdev[0])
            stdevData.append(meanStdev[1])
        
        plt.title("%s vs %s" % (ylabel, xlabel))
        #plt.plot(xData, meanData, color='b')
        plt.errorbar(xData, meanData, yerr=stdevData, linestyle='-', color=colour, capsize=3)
        #plt.ylabel(ylabel)
        #plt.xlabel(xlabel)

        
batchPath = r'D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE Builds\Results\rtrTests'
#batchPath = r'C:\Users\tom_j\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE Builds\Results\rtrTests'

oldNest = 0
worstNest = 1
bestNest = 2 

dataExtractor = DataExtractor(batchPath)
#dataExtractor.print_data(True)
#print(dataExtractor.dataRtrTrue['Absolute Cohesion'])

dataPlotter = DataPlotter(dataExtractor.plotDictRtrTrue, dataExtractor.plotDictRtrFalse)   