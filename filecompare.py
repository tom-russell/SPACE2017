path1 = r"D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE2017\Results\nestAllegiance_q2_r16_4\ants_detail.txt"
path2 = r"D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files\SPACE2017\Results\nestAllegiance_q2_r16_5\ants_detail.txt"

file1 = open(path1)
file2 = open(path2)
lines1 = file1.readlines()
lines2 = file2.readlines()
file1.close()
file2.close()

for i in range(0, len(lines1)):
    if (lines1[i] != lines2[i]):
        print(lines1[i][:-1])
        print(lines2[i])