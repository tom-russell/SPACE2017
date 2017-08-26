primaryFilePath = r"D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE2017\Results\EditorBuildDTest_1\ants_detail.txt"
otherFilePaths = [
    r"D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE Builds\Results\EditorBuildDTest_2\ants_detail.txt"
]
numFiles = len(otherFilePaths)
fileErrors = []

primaryFile = open(primaryFilePath)
fileLines = primaryFile.readlines()
primaryFile.close()

otherFileLines = []
for i in range(0, numFiles):
    file = open(otherFilePaths[i])
    otherFileLines.append(file.readlines())
    fileErrors.append(0)
    file.close()
    
    if len(otherFileLines[i]) > len(fileLines):
        fileErrors[i] += abs(len(otherFileLines[i]) - len(fileLines))
        
for i in range(0, len(fileLines)):
    
    for j in range(0, numFiles):
        if fileLines[i] != otherFileLines[j][i]:
            fileErrors[i] += 1
print(fileErrors)
