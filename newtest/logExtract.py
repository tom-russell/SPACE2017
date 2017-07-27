file = open("space_Data\output_log.txt")
lines = file.readlines()

for line in lines:
    if line[:3] == "ID=":
        print(line[:-1])