from tkinter import Tk, Entry, Label, Button, Checkbutton, BooleanVar, StringVar, IntVar, DoubleVar, filedialog
import os

class BatchGeneratorGUI:
    def __init__(self, master):
        self.master = master
        master.title("SPACE Batch File Generator")

        Label(self.master, text= "Setting:").grid(row=0, column=0)
        Label(self.master, text= "Value:").grid(row=0, column=1)
        Label(self.master, text= "Vary Setting:").grid(row=0, column=2)
        
        self.rowCount = 1
        
        self.name = StringVar()
        self.seed = IntVar()
        self.timescale = IntVar()
        self.fluctuatingTimescale = BooleanVar()
        self.maxRunTime = IntVar()
        self.colonySize = StringVar()
        self.qThreshold = StringVar()
        self.pActive = DoubleVar()
        self.oldNestQ = DoubleVar()
        self.newNest1Q = DoubleVar()
        self.newNest2Q = DoubleVar()
        self.pheromones = BooleanVar()
        self.reverseTandem = BooleanVar()
        self.outTickRate = IntVar()
        self.outEmigration = BooleanVar()
        self.outColony = BooleanVar()
        self.outDeltas = BooleanVar()
        self.outStates = BooleanVar()
        self.outDetail = BooleanVar()
        self.outLegacy = BooleanVar()
        self.outDebug = BooleanVar()
        self.arenaFilePath = StringVar()
        
        self.addTextEntryToGrid("Base Experiment Name", self.name, False)
        self.addTextEntryToGrid("Random Seed", self.seed, True)
        self.addTextEntryToGrid("Starting Time Scale", self.timescale, True)
        self.addCheckButtonToGrid("Fluctuating Time Scale", self.fluctuatingTimescale, True)
        self.addTextEntryToGrid("Max Simulation Run Time (s)", self.maxRunTime, False)
        self.addTextEntryToGrid("Colony Size", self.colonySize, True)
        self.addTextEntryToGrid("Quorum Threshold", self.qThreshold, True)
        self.addTextEntryToGrid("Proportion Active", self.pActive, True)
        self.addTextEntryToGrid("Starting Nest Quality", self.oldNestQ, True)
        self.addTextEntryToGrid("First New Nest Quality", self.newNest1Q, True)
        self.addTextEntryToGrid("Second New Nest Quality", self.newNest2Q, True)
        self.addCheckButtonToGrid("Ants Lay Pheromones", self.pheromones, True)
        self.addCheckButtonToGrid("Ants Reverse Tandem Run", self.reverseTandem, True)
        self.addTextEntryToGrid("Output Tick Rate", self.outTickRate, False)
        self.addCheckButtonToGrid("Output Emigration Data", self.outEmigration, False)
        self.addCheckButtonToGrid("Output Colony Data", self.outColony, False)
        self.addCheckButtonToGrid("Output Ant Deltas", self.outDeltas, False)
        self.addCheckButtonToGrid("Output Ant State Distribution", self.outStates, False)
        self.addCheckButtonToGrid("Output Ant Detail", self.outDetail, False)
        self.addCheckButtonToGrid("Output Legacy Data", self.outLegacy, False)
        self.addCheckButtonToGrid("Output Ant Debug", self.outDebug, False)
        self.addTextEntryToGrid("Arena File Path", self.arenaFilePath, True)
        Button(master, text="Browse Arena Path", command=self.setArenaPath).grid(row=self.nextRow(), column=1, padx=5, pady=2)
        
        Button(master, text="Continue...", command=self.continueToNext, padx=50, pady=20).grid(row=self.nextRow(), column=0, columnspan=3, padx=15, pady=15, sticky='ew')
        
    def setArenaPath(self):
        path = filedialog.askopenfilename(filetypes = (("XML files","*.xml"),("all files","*.*")))
        self.arenaFilePath.set(path)
        
    def nextRow(self):
        self.rowCount += 1
        return self.rowCount - 1
        
    def addTextEntryToGrid(self, displayText, var, varyOption):
        thisRow = self.nextRow()
        
        Label(self.master, text=displayText, justify='left').grid(row=thisRow, column=0, sticky='e', padx=5, pady=2)
        self.entry = Entry(self.master, textvariable=var)
        self.entry.grid(row=thisRow, column=1, padx=5, pady=2)
        if varyOption is True:
            Checkbutton(self.master, command=lambda e=self.entry, v=var: self.disableWidget(e,v)).grid(row=thisRow, column=2, padx=5, pady=2)
    
    def addCheckButtonToGrid(self, displayText, var, varyOption):
        thisRow = self.nextRow()
        
        Label(self.master, text=displayText).grid(row=thisRow, column=0, sticky='e', padx=5, pady=2)
        self.cb = Checkbutton(self.master, variable=var)
        self.cb.grid(row=thisRow, column=1, padx=5, pady=2)
        if varyOption is True:
            Checkbutton(self.master, command=lambda e=self.cb, v=var: self.disableWidget(e,v)).grid(row=thisRow, column=2, padx=5, pady=2)
        
    def disableWidget(self, widget, var):
        if widget.cget('state') == 'disabled':
            widget.config(state='normal')
        else:
            widget.config(state='disabled')
            
    def continueToNext(self):
        #settings = SettingsObject(self.name, self.seed, self.timescale, self.fluctuatingTimescale, self.maxRunTime, self.colonySize, self.qThreshold, self.pActive, self.oldNestQ, self.newNest1Q, self.newNest2Q, self.pheromones, self.reverseTandem, self.outTickRate, self.outEmigration, self.outColony, self.outDeltas, self.outStates, self.outDetail, self.outLegacy, self.outDebug, self.arenaFilePath)
        #settings.createFile("testSettingsFile")
        
        self.settingsList = []
        self.variableSettings = [self.seed, self.timescale, self.colonySize, self.qThreshold, self.pActive]
        self.generateSettingsArray()
        
    def generateSettingsArray(self):
        for seedValue in self.getValues(self.seed):
            for timescaleValue in self.getValues(self.timescale):
                for colonySizeValue in self.getValues(self.colonySize):
                    for qThresholdValue in self.getValues(self.qThreshold):
                        for pActiveValue in self.getValues(self.pActive):
                            #print(self.getValues(self.qThreshold))
                            print("seed=" + seedValue + ",timescale=" + timescaleValue + ",colonySize=" + colonySizeValue + ",qThreshold=" + qThresholdValue + ",pActive=" + pActiveValue)
            
    def getValues(self, input):
        return str(input.get()).split(',')
        
class SettingsObject:
    def __init__(self, name, seed, timescale, fluctuatingTimescale, maxRunTime, colonySize, qThreshold, pActive, oldNestQ, newNest1Q, newNest2Q, pheromones, reverseTandem, outTickRate, outEmigration, outColony, outDeltas, outStates, outDetail, outLegacy, outDebug, arenaFilePath):
        self.name = name.get()
        self.seed = seed.get()
        self.timescale = timescale.get()
        self.fluctuatingTimescale = fluctuatingTimescale.get()
        self.maxRunTime = maxRunTime.get()
        self.colonySize = colonySize.get()
        self.qThreshold = qThreshold.get()
        self.pActive = pActive.get()
        self.oldNestQ = oldNestQ.get()
        self.newNest1Q = newNest1Q.get()
        self.newNest2Q = newNest2Q.get()
        self.pheromones = pheromones.get()
        self.reverseTandem = reverseTandem.get()
        self.outTickRate = outTickRate.get()
        self.outEmigration = outEmigration.get()
        self.outColony = outColony.get()
        self.outDeltas = outDeltas.get()
        self.outStates = outStates.get()
        self.outDetail = outDetail.get()
        self.outLegacy = outLegacy.get()
        self.outDebug = outDebug.get()
        self.arenaFilePath = arenaFilePath.get()
        
    def createFile(self, fileName):
        file = open(fileName + ".xml", 'w')
        
        file.write('<?xml version="1.0" encoding="utf-8"?>\n<SimulationSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n')
        file.write("\t<ArenaName>" + os.path.basename(self.arenaFilePath[:-4]) + "</ArenaName>\n")
        file.write("\t<ArenaFilename>" + self.arenaFilePath + "</ArenaFilename>\n")
        file.write(self.xmlStringWithValue("ExperimentName", self.name))
        file.write(self.xmlStringWithValue("RandomSeed", self.seed))
        file.write(self.xmlStringWithValue("ColonySize", self.colonySize))
        file.write(self.xmlStringWithValue("QuorumThreshold", self.qThreshold))
        file.write(self.xmlStringWithValue("ProportionActive", self.pActive))
        file.write(self.xmlStringWithValue("StartingNestQuality", self.oldNestQ))
        file.write(self.xmlStringWithValue("FirstNewNestQuality", self.newNest1Q))
        file.write(self.xmlStringWithValue("SecondNewNestQuality", self.newNest2Q))
        file.write(self.xmlStringWithValue("StartingTimeScale", self.timescale))
        file.write(self.xmlStringWithValue("MaximumSimulationRunTime", self.maxRunTime))
        file.write(self.xmlStringWithValue("AntsLayPheromones", self.pheromones))
        file.write(self.xmlStringWithValue("AntReverseTandemRun", self.reverseTandem))
        file.write(self.xmlStringWithValue("OutputTickRate", self.outTickRate))
        file.write(self.xmlStringWithValue("OutputEmigrationData", self.outEmigration))
        file.write(self.xmlStringWithValue("OutputColonyData", self.outColony))
        file.write(self.xmlStringWithValue("OutputAntDelta", self.outDeltas))
        file.write(self.xmlStringWithValue("OutputAntStateDistribution", self.outStates))
        file.write(self.xmlStringWithValue("OutputAntDetail", self.outDetail))
        file.write(self.xmlStringWithValue("OutputLegacyData", self.outLegacy))
        file.write(self.xmlStringWithValue("OutputAntDebug", self.outDebug))
        file.write("</SimulationSettings>")
        
        file.close()
    
    def xmlStringWithValue(self, tag, value):
        return "\t<" + tag + ">\n" + "\t\t<Value>" + str(value).lower() + "</Value>\n" + "\t</" + tag + ">\n"
        
root = Tk()
gui = BatchGeneratorGUI(root)
root.mainloop()