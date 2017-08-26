import os


class DataExtractor:
    def __init__(self, path, best_nest, max_sim_time):

        self.path = path
        self.dirs = [d for d in os.listdir(self.path) if os.path.isdir(os.path.join(self.path, d))]

        self.best_nest = best_nest
        self.max_sim_time = int(max_sim_time) * 60  # converted to minutes

        # The data for each simulation is stored as an dictionary element within the data_set
        self.data_set = []

    def extract_data(self):
        invalid_files = 0
        unfinished_sims = 0

        # For each directory (/simulation result) create a 'data point dictionary' in the data set
        for experimentDir in self.dirs:
            file_path = os.path.join(self.path, experimentDir, "endSimData.txt")

            # If the file is empty, inaccessible or doesn't exist, skip this directory
            if not os.path.exists(file_path) or os.path.getsize(file_path) == 0:
                invalid_files += 1
                continue

            file = open(file_path, 'r')
            data_lines = file.readlines()

            print(int(data_lines[-1].split(": ")[1]))
            print(self.max_sim_time )
            # If the simulation reached the max possible run time, it did not complete and so is removed
            if int(data_lines[-1].split(": ")[1]) == self.max_sim_time:
                unfinished_sims += 1
                continue

            data_point = dict()
            self.data_set.append(data_point)

            # Add each of the parameters from the experiment name (e.g. quorum=20, RTR=false)
            experiment_string = experimentDir.split('_')

            for parameter in experiment_string[1:]:
                key_value = parameter.replace("\n", "").split("=")
                data_name = key_value[0].lower()
                # If the value is a number, save as a number, else save it as a float
                try:
                    data_point[data_name] = float(key_value[1])
                except ValueError:
                    data_point[data_name] = str(key_value[1])

            # Add the data from the endSimData file
            for line in range(1, len(data_lines)):
                data_line = data_lines[line].split(': ')
                data_name = data_line[0].lower()
                data_value = float(data_line[1][:-1])
                data_point[data_name] = data_value

            # Add the other data types calculated from the existing ones
            implementation_time = data_point['end of emigration time'] - data_point['first carry time']
            data_point['implementation time'] = implementation_time

            nest_allegiances = list(map(int, data_lines[0][:-1].split(',')))
            total_emigrated = sum(nest_allegiances) - nest_allegiances[0]
            absolute_emigration_cohesion = float(nest_allegiances[self.best_nest]) / 200
            relative_emigration_cohesion = float(nest_allegiances[self.best_nest]) / total_emigrated
            data_point['absolute cohesion'] = absolute_emigration_cohesion
            data_point['relative cohesion'] = relative_emigration_cohesion

            in_wrong_nests = total_emigrated - nest_allegiances[self.best_nest]
            accuracy = 1 if nest_allegiances[self.best_nest] > in_wrong_nests else 0
            data_point['accuracy'] = accuracy

        return invalid_files, unfinished_sims
