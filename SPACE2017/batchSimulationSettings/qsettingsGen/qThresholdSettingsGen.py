import os, random

name = "nestAllegiance"
qThreshold = [0,2,5,8,10,12,15,18,20]
repeats = 20
timescale = 50
maxRunTime = 120
outEndSimData = True;
arenaName = "Equidistant"
arenaFilePath = "D:/Libraries/Documents/Google Drive/Bristol Work/Research Skills/Project Unity Files/My Files/SPACE2017/space/Arenas/Equidistant.xml"

def createFile(q, repeat):
        thisName = name + "_q" + str(q) + "_r" + str(repeat)
        file = open(thisName + ".xml", 'w')
        print("_q" + str(q) + "_r" + str(repeat))
        file.write('<?xml version="1.0" encoding="utf-8"?>\n<SimulationSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n')
        file.write("\t<ArenaName>" + arenaName + "</ArenaName>\n")
        file.write("\t<ArenaFilename>" + arenaFilePath + "</ArenaFilename>\n")
        file.write(xmlStringWithValue("ExperimentName", thisName, False))
        file.write(xmlStringWithValue("RandomSeed", random.randint(0, 2147483647), False))
        file.write(xmlStringWithValue("QuorumThreshold", q, False))
        file.write(xmlStringWithValue("StartingTimeScale", timescale, False))
        file.write(xmlStringWithValue("MaximumSimulationRunTime", maxRunTime, False))
        file.write(xmlStringWithValue("OutputEndSimData", outEndSimData, True))
        file.write("</SimulationSettings>")
        
        file.close()
    
def xmlStringWithValue(tag, value, toLower):
    value = str(value)
    if toLower is True:
        value = value.lower()
        
    
    return "\t<" + tag + ">\n" + "\t\t<Value>" + value + "</Value>\n" + "\t</" + tag + ">\n"
    
for q in qThreshold:
    for repeat in range(0, repeats):
        createFile(q, repeat)