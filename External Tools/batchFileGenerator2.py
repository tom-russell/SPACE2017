import os
import random

def AddSetting(name, value):
    return "\t<{0}>\n\t\t<Value>{1}</Value>\n\t</{0}>\n".format(name, value)

batchPath = "arenaSizeTest"
INTMAX = 2147483647
arenaDirectory = r"D:/Libraries/Documents/Google Drive/Bristol Work/Research Skills/Project Unity Files/My Files/SPACE2017/Arenas/"
# arenaName = "Equidistant"

if not os.path.exists(batchPath):
    os.makedirs(batchPath)

for repeat in range(0, 3):
    for quorum in [0, 3, 6, 9, 12, 16, 20]:
        for arenaFile in ['SmallArena']:
            fileName = batchPath + '_quorum=' + str(quorum).rjust(2, '0') + '_repeat=' + str(repeat).rjust(2, '0') + '_arena=' + arenaFile + '.xml'
            filePath = os.path.join(batchPath, fileName)
            
            file = open(filePath, 'w')
            file.write('<?xml version="1.0" encoding="utf-8"?>\n<SimulationSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema">\n')
            file.write('\t<ArenaName>%s</ArenaName>\n' % arenaFile)
            file.write('\t<ArenaFilename>%s%s.xml</ArenaFilename>\n' % (arenaDirectory, arenaFile))
            
            file.write(AddSetting("ExperimentName", fileName[:-4]))
            file.write(AddSetting("RandomSeed", random.randint(0, INTMAX)))
            file.write(AddSetting("QuorumThreshold", quorum))
            file.write(AddSetting("StartingTimeScale", 50))
            file.write(AddSetting("MaximumSimulationRunTime", 150))
            file.write(AddSetting("WaitNewNestFactor", 1))
            file.write(AddSetting("OutputEndSimData", "true"))
            file.write(AddSetting("OutputColonyData", "true"))
            file.write(AddSetting("OutputAntStateDistribution", "true"))
            
            file.write('</SimulationSettings>')

            file.close()