import os
import matplotlib.pyplot as plt
from cycler import cycler
import numpy as np
from enum import Enum


class DataExtractor:
    def __init__(self, path, best_nest, worst_nest, max_sim_time):

        self.path = path
        self.dirs = [d for d in os.listdir(self.path) if os.path.isdir(os.path.join(self.path, d))]

        self.best_nest = best_nest
        self.worst_nest = worst_nest
        self.max_sim_time = str(max_sim_time)

        # The data for each simulation is stored as an dictionary element within the data_set
        self.data_set = []

        # Extract the data
        self.extract_all_data()

    def extract_all_data(self):
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

            # If the simulation reached the max possible run time, it did not complete and so is removed
            if data_lines[-1].split(": ")[1] == self.max_sim_time:
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
            total_emigrated = float(nest_allegiances[self.worst_nest]) + float(nest_allegiances[self.best_nest])
            absolute_emigration_cohesion = float(nest_allegiances[self.best_nest]) / 200
            relative_emigration_cohesion = float(nest_allegiances[self.best_nest]) / total_emigrated
            data_point['absolute cohesion'] = absolute_emigration_cohesion
            data_point['relative cohesion'] = relative_emigration_cohesion

            accuracy = 1 if nest_allegiances[self.best_nest] > nest_allegiances[self.worst_nest] else 0
            data_point['colony accuracy'] = accuracy

        print("%s simulations had missing or blank files" % invalid_files)
        print("%s simulations took longer than the maximum time and so were removed." % unfinished_sims)

    def print_data(self):
        for data_point in self.data_set:
            out_string = ""

            for key, value in data_point.items():
                out_string += "%s=%s " % (key, value)

            print(out_string)


class DataPlotter:
    def __init__(self, data_set):
        self.data_set = data_set

    def plot_data(self, plot_parameters):
        # Plot the correct graph style for the given inputs
        for data_constraints in plot_parameters.data_constraints_series:
            if plot_parameters.graph_style is GraphStyle.LINE:
                self.plot_line(plot_parameters, data_constraints)

            if plot_parameters.graph_style is GraphStyle.BAR:
                self.plot_bar(plot_parameters, data_constraints)

            if plot_parameters.graph_style is GraphStyle.SCATTER:
                self.plot_scatter(plot_parameters, data_constraints)

        plt.xlabel(plot_parameters.labels[0])
        plt.ylabel(plot_parameters.labels[1])
        plt.legend(loc='upper left')

    def plot_line(self, pp, data_constraints):
        plot_data = self.sort_plot_data(pp, data_constraints)

        x_mean = [entry['x_mean'] for entry in plot_data.values()]
        y_mean = [entry['y_mean'] for entry in plot_data.values()]
        y_stddev = [entry['y_stddev'] for entry in plot_data.values()]

        plt.errorbar(x_mean, y_mean, yerr=y_stddev, linestyle='-', capsize=3, label=self.create_label(data_constraints))

    def plot_bar(self, pp, data_constraints):
        plot_data = self.sort_plot_data(pp, data_constraints)

        x_mean = [entry['x_mean'] for entry in plot_data.values()]
        y_mean = [entry['y_mean'] for entry in plot_data.values()]
        x_stddev = [entry['x_stddev'] for entry in plot_data.values()]
        y_stddev = [entry['y_stddev'] for entry in plot_data.values()]

        label = self.create_label(data_constraints) + ', ' + pp.y_type
        plt.bar(x_mean, y_mean, yerr=y_stddev, label=label, alpha=0.5)

    def plot_scatter(self, pp, data_constraints):
        plot_data = self.sort_plot_data(pp, data_constraints)

        x_mean = [entry['x_mean'] for entry in plot_data.values()]
        y_mean = [entry['y_mean'] for entry in plot_data.values()]
        x_stddev = [entry['x_stddev'] for entry in plot_data.values()]
        y_stddev = [entry['y_stddev'] for entry in plot_data.values()]

        plt.scatter(x_mean, y_mean,  s=8, label=self.create_label(data_constraints))
        # plt.errorbar(x_mean, y_mean, xerr=x_stddev, yerr=y_stddev)

        for key, value in plot_data.items():
            plt.annotate(int(key), xy=(value['x_mean'], value['y_mean']), xytext=(-4, 3), textcoords='offset pixels')

    def sort_plot_data(self, pp, data_constraints):
        plot_data = dict()  # Collecting the values together, so the mean and stddev can be calculated after

        for data in self.data_set:
            if self.check_constraint(data, data_constraints) is False:
                continue

            x_value = float(data[pp.x_type])
            y_value = float(data[pp.y_type])
            self.add_data(plot_data, x_value, y_value, data[pp.group_type])

        # Calculate the means and standard deviation for each x value
        for entry in plot_data:
            x_list = [repeat['x'] for repeat in plot_data[entry]]
            y_list = [repeat['y'] for repeat in plot_data[entry]]

            x_mean = sum(x_list) / len(x_list)
            y_mean = sum(y_list) / len(y_list)
            x_stddev = np.std(x_list)
            y_stddev = np.std(y_list)
            plot_data[entry] = {'x_mean': x_mean, 'x_stddev': x_stddev, 'y_mean': y_mean, 'y_stddev': y_stddev}

        return plot_data

    @staticmethod
    def check_constraint(data, data_constraints):
        for c in data_constraints:
            operator = c.operator
            actual_val = data[c.constraint]
            required_val = c.value

            if operator == '=' and actual_val != required_val:
                return False
            elif operator == '!=' and actual_val == required_val:
                return False
            elif operator == '>' and actual_val <= required_val:
                return False
            elif operator == '<' and actual_val >= required_val:
                return False
            elif operator == '>=' and actual_val < required_val:
                return False
            elif operator == '<=' and actual_val > required_val:
                return False

        return True

    @staticmethod
    def add_data(plot_data, x_value, y_value, group_value):

        if group_value not in plot_data:
            plot_data[group_value] = []

        new_entry = {'x': x_value, 'y': y_value}
        plot_data[group_value].append(new_entry)

    @staticmethod
    def create_label(data_constraints):
        label = ""
        for dc in data_constraints:
            label += "%s %s %s," % (dc.constraint, dc.operator, dc.value)

        return label[:-1]


class GraphStyle(Enum):
    LINE, BAR, SCATTER = range(0, 3)


class PlotParameters:
    def __init__(self, graph_style, x_type, y_type, group_type, data_constraints_series, labels):
        self.graph_style = graph_style
        self.x_type = x_type
        self.y_type = y_type
        self.group_type = group_type
        self.data_constraints_series = data_constraints_series
        self.labels = labels


class Constraint:
    def __init__(self, constraint, operator, value):
        self.constraint = constraint
        self.operator = operator
        self.value = value


def main():
    # my_files_path = r'D:\Libraries\Documents\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files'
    my_files_path = r'C:\Users\tom_j\Google Drive\Bristol Work\Research Skills\Project Unity Files\My Files'
    batch_path = os.path.join(my_files_path, r'SPACE Builds\Results\rtrTests')

    old_nest = 0
    worst_nest = 1
    best_nest = 2
    max_sim_time = 120 * 60
    plt.rc('axes', prop_cycle=(cycler('color', ['b', 'r', 'g', 'c', 'm', 'y', 'k', 'w'])))

    data_extractor = DataExtractor(batch_path, best_nest, worst_nest, max_sim_time)

    data_plotter = DataPlotter(data_extractor.data_set)

    rtr_on = {Constraint('rtr', '=', 'true')}
    rtr_off = {Constraint('rtr', '=', 'false')}

    plt.subplot(3, 3, 1)
    pp = PlotParameters(GraphStyle.LINE, 'quorum', 'end of emigration time', 'quorum',
                        [rtr_on, rtr_off], ['Quorum', 'Speed'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 2)
    pp = PlotParameters(GraphStyle.LINE, 'quorum', 'discovery time', 'quorum',
                        [rtr_on, rtr_off], ['Quorum', 'Discovery Time'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 3)
    pp = PlotParameters(GraphStyle.LINE, 'quorum', 'first carry time', 'quorum',
                        [rtr_on, rtr_off], ['Quorum', 'Decision Time'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 4)
    pp = PlotParameters(GraphStyle.LINE, 'quorum', 'implementation time', 'quorum',
                        [rtr_on, rtr_off], ['Quorum', 'Implementation Time'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 5)
    pp = PlotParameters(GraphStyle.LINE, 'quorum', 'absolute cohesion', 'quorum',
                        [rtr_on, rtr_off], ['Quorum', 'Cohesion'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 6)
    pp = PlotParameters(GraphStyle.BAR, 'quorum', 'ftrs', 'quorum',
                        [rtr_on], ['Quorum', 'Number of Runs'])
    data_plotter.plot_data(pp)
    pp = PlotParameters(GraphStyle.BAR, 'quorum', 'rtrs', 'quorum',
                        [rtr_on], ['Quorum', 'Number of Runs'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 7)
    pp = PlotParameters(GraphStyle.SCATTER, 'end of emigration time', 'absolute cohesion', 'quorum',
                        [rtr_on], ['Speed', 'Cohesion'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 8)
    pp = PlotParameters(GraphStyle.SCATTER, 'end of emigration time', 'absolute cohesion', 'quorum',
                        [rtr_off], ['Speed', 'Cohesion'])
    data_plotter.plot_data(pp)

    plt.subplot(3, 3, 9)
    pp = PlotParameters(GraphStyle.SCATTER, 'end of emigration time', 'absolute cohesion', 'quorum',
                        [rtr_on, rtr_off], ['Speed', 'Cohesion'])
    data_plotter.plot_data(pp)

    plt.show()


if __name__ == '__main__':
    main()
