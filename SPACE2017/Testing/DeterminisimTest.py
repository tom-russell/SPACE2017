import os
import sys
import numpy as np
import matplotlib.pyplot as plt
import csv

class DeterminismTest:
    # Set the correct path names for the output files
    baseResultsPath = os.path.join(os.pardir, "Results", "DeterminismTest", "dtest_seed")
    filePathVariants = ["_timescale01", "_timescale02", "_timescale05", "_timescale10", "_timescale15", "_timescaleRANDOM"]
    
    def __init__(self):
        pass      
    
    def analyseResultSets(self):
        speedUpFactors = []
        
        # Test the results for each of the 3 seeds
        for seedNumber in range (1, 4):
            print("analysing seed " + str(seedNumber) + " data...") 
            speedUpFactors.append(self.extractExecutionTimes(seedNumber))
            self.analyseAntDetails(seedNumber)
            
            print("seed " + str(seedNumber) + " complete.\n")
            
        plt.plot([1, 2, 5, 10, 15], speedUpFactors[0], 'r')
        plt.plot([1, 2, 5, 10, 15], speedUpFactors[1], 'b')
        plt.plot([1, 2, 5, 10, 15], speedUpFactors[2], 'g')
        plt.xlabel("Unity Timescale")
        plt.ylabel("Simulation Speed Increase (simulation time / execution time)")
        plt.axis([0, 16, 0, 10])
        plt.grid(True)
        plt.show()
        
    def extractExecutionTimes(self, seedNumber):
        speedUpFactor = []
        
        # Extract the execution & simulation times for each simulation result (ignore the random timescale data for this part)
        for i in range (0, 5):
            execFileName = os.path.join(self.baseResultsPath + str(seedNumber) + self.filePathVariants[i], "execution.txt")
            
            try:
                executionFile = open(execFileName, "r")
                fileLines = executionFile.readlines()
                executionTime = fileLines[2][20:].strip('\n').split(":")
                simulationTime = fileLines[3][21:].strip('\n').split(":")
                execTimeSecs = int(executionTime[0]) * 3600 + int(executionTime[1]) * 60 + int(executionTime[2])
                simTimeSecs = int(simulationTime[0]) * 3600 + int(simulationTime[1]) * 60 + int(simulationTime[2])
                speedUpFactor.append(simTimeSecs / execTimeSecs)
                
            except FileNotFoundError:
                dataSetIncomplete(execFileName)
                
            finally:
                executionFile.close()
                
        return speedUpFactor
    
    def analyseAntDetails(self, seedNumber):
        # Since the values below are compared to timescale 1, timescale 1 (element 0) will always be 0
        stateErrorCount = [0, 0, 0, 0, 0, 0]                    # Counts the number of errors (different states or additional/missing timesteps) when comparing timescale x1 to other timescale data
        totalPosDifference = [0.0, 0.0, 0.0, 0.0, 0.0, 0.0]     # Calculates the sum of each position difference at each timesteps for the same ant at different timescales
        
        try:
            # load each of the different timescale data files and store the reference in an array
            timeScaleFiles = []
            for filePathVariant in self.filePathVariants:
                detailFileName = os.path.join(self.baseResultsPath + str(seedNumber) + filePathVariant, "ants_detail.txt")
                timeScaleFiles.append(open(detailFileName, "r"))
                timeScaleFiles[-1].readline()   # The first line is ignored since it contains only the column names
                
            for lineStr in iter(timeScaleFiles[0].readline, ''): #((lineStr = timeScaleFiles[0].readline()) != "")
                ts1Row = lineStr.strip('\n').split(',')
                
                # Compare timescale1 to each of the other timeScales
                for i in range (1, 6):
                    otherTsRow = timeScaleFiles[i].readline().strip('\n').split(',')

                    # add one error if this timescale has missing timesteps (one error will be added for each ant)
                    if len(otherTsRow) <= 1:
                        stateErrorCount[i] += 1
                        continue
                    
                    # Compare the ant state at timescale 1 to the state at the same timestep in the other timescale
                    if ts1Row[2] != otherTsRow[2]: 
                        stateErrorCount[i] += 1
                    # Compare x/z position values, if there is a difference calculate the difference between the two coordinates and add to the total
                    if ts1Row[3] != otherTsRow[3] or ts1Row[4] != otherTsRow[4] or ts1Row[5] != otherTsRow[5]:
                        stateErrorCount[i] += 1
                        pos1 = [float(ts1Row[3]), float(ts1Row[4]), float(ts1Row[5])]
                        pos2 = [float(otherTsRow[3]), float(otherTsRow[4]), float(otherTsRow[5])]
                        #totalPosDifference += np.sqrt(((a - b) ** 2).sum(1))
                        totalPosDifference[i] += np.sqrt((pos1[0]-pos2[0])**2 + (pos1[1]-pos2[1])**2 + (pos1[2]-pos2[2])**2) 
                        print(np.sqrt((pos1[0]-pos2[0])**2 + (pos1[1]-pos2[1])**2 + (pos1[2]-pos2[2])**2) )
            
            print(stateErrorCount)
            print(totalPosDifference)
            
            # Timescale of 1 is assumed to be the valid version of the data, so the other timescales are compared to it
                
        except FileNotFoundError:
            dataSetIncomplete(detailFileName)
            
        finally:
            for file in timeScaleFiles:
                file.close()
                
    def dataSetIncomplete(fileName):
        print("Data set incomplete, file: \"" + fileName + "\" was not found. Please re-run the tests.")
        sys.exit()

dTests = DeterminismTest()
dTests.analyseResultSets()