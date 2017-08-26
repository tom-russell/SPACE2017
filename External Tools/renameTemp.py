import os
import pprint

def insert(dict, key):
    if key not in dict:
        dict[key] = 1
    else:
        dict[key] += 1

path = r'D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE Builds\Results\recWaitFactorTests'
dirs = [d for d in os.listdir(path) if os.path.isdir(os.path.join(path, d))]

parts = [dict(), dict(), dict()]

for dir in dirs:
    name = dir.split('_')
    insert(parts[0], name[1])
    insert(parts[1], name[2])
    insert(parts[2], name[3])
    '''name[0] = 'recWaitTests'
    name[-1] = 'waitFactor=0'
    newname = '_'.join(name)
    os.rename(os.path.join(path, dir), os.path.join(path, newname))'''

pp = pprint.PrettyPrinter(indent=4)
    
for part in parts:
    pp.pprint(part)