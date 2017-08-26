import matplotlib.pyplot as plt
import numpy as np
from enum import Enum
from cycler import cycler
import scipy.stats as stats

worst_nest = 1
best_nest = 2
max_sim_time = 120 * 60


class DataPlotter:
    def __init__(self, data_set):
        self.data_set = data_set
        self.fig = plt.Figure
        plt.rc('axes', prop_cycle=(cycler('color', ['b', 'r', 'g', 'c', 'm', 'y', 'k', 'w'])))

        self.bar_plot_count = 0  # Used to space bar chart bars correctly when plotting multiple series on the same chart
        self.bar_plots_total = 0

        self.trim_data = False
        self.trim_data_percent = 10.0
        self.default_style = GraphStyle.LINE
        self.error_bars = '1 SD'

    def plot_data(self, plot_parameters, constraints):
        style = plot_parameters.graph_style
        if style == GraphStyle.DEFAULT:
            style = self.default_style

        # Plot the correct graph style for the given inputs
        if style is GraphStyle.LINE:
            self.plot_line(plot_parameters, constraints)

        elif style is GraphStyle.BAR:
            self.plot_bar(plot_parameters, constraints)
        elif style is GraphStyle.SCATTER_GROUPED:
            self.plot_grouped_scatter(plot_parameters, constraints)
        elif style is GraphStyle.SCATTER_BASIC:
            self.plot_basic_scatter(plot_parameters, constraints)
        elif style is GraphStyle.BOX_PLOT:
            self.plot_box_plot(plot_parameters, constraints)

        plt.xlabel(plot_parameters.labels[0])
        plt.ylabel(plot_parameters.labels[1])
        plt.legend(loc='upper left')

    def plot_line(self, pp, constraints):
        plot_data = self.sort_plot_data(pp, constraints)
        if len(plot_data) == 0:
            return

        x_mean = [entry['x_mean'] for entry in plot_data.values()]
        y_mean = [entry['y_mean'] for entry in plot_data.values()]
        y_error = [entry['y_error'] for entry in plot_data.values()]

        plt.errorbar(x_mean, y_mean, yerr=y_error, linestyle='-', capsize=3, label=self.create_label(constraints))

    def plot_bar(self, pp, constraints):
        plot_data = self.sort_plot_data(pp, constraints)
        if len(plot_data) == 0:
            return

        x_mean = [entry['x_mean'] for entry in plot_data.values()]
        y_mean = [entry['y_mean'] for entry in plot_data.values()]
        y_error = [entry['y_error'] for entry in plot_data.values()]

        label = self.create_label(constraints) + ', ' + pp.y_type
        width = 1 / self.bar_plots_total
        spacing = width * (self.bar_plot_count - 1)

        plt.bar([x + spacing for x in x_mean], y_mean, width=width, yerr=y_error, label=label, alpha=0.5)

    def plot_grouped_scatter(self, pp, constraints):
        plot_data = self.sort_plot_data(pp, constraints)
        if len(plot_data) == 0:
            return

        x_mean = [entry['x_mean'] for entry in plot_data.values()]
        y_mean = [entry['y_mean'] for entry in plot_data.values()]

        plt.scatter(x_mean, y_mean,  s=8, label=self.create_label(constraints))
        for key, value in plot_data.items():
            plt.annotate(int(key), xy=(value['x_mean'], value['y_mean']), xytext=(-4, 3), textcoords='offset pixels')

    def plot_basic_scatter(self, pp, constraints):
        plot_data = self.sort_plot_data(pp, constraints, group=False)
        if len(plot_data) == 0:
            return

        x = [entry['x'] for entry in plot_data['none']]
        y = [entry['y'] for entry in plot_data['none']]

        plt.scatter(x, y, s=8, label=self.create_label(constraints))

    def plot_box_plot(self, pp, constraints):
        plot_data, x_positions = self.sort_plot_data(pp, constraints, calculate_means=False)
        if len(plot_data) == 0:
            return

        plt.boxplot(plot_data, positions=x_positions, whis='range')

    def sort_plot_data(self, pp, constraints, calculate_means=True, group=True):
        plot_data = dict()

        # Collect required data values together, ignoring data points that break the constraints
        for data in self.data_set:
            if self.check_constraint(data, constraints) is False:
                continue

            x_value = float(data[pp.x_type])
            y_value = float(data[pp.y_type])
            if not group:
                self.add_data(plot_data, x_value, y_value, 'none')
            else:
                self.add_data(plot_data, x_value, y_value, data[pp.group_type])

        # If grouping data is not required, return the raw values.
        if not group:
            return plot_data
        elif self.trim_data is True and self.trim_data_percent > 0:
            for entry in plot_data:
                count = len(plot_data[entry])
                num_to_remove = int(round(count * (self.trim_data_percent / 100)))

                for i in range(0, num_to_remove):
                    plot_data[entry].remove(max(plot_data[entry], key=lambda d: d['y']))
                    plot_data[entry].remove(min(plot_data[entry], key=lambda d: d['y']))

        raw_data = []
        raw_data_positions = []
        # Calculate the means, standard deviations (or box plot data) for each grouped x value
        for entry in plot_data:
            x_list = [repeat['x'] for repeat in plot_data[entry]]
            y_list = [repeat['y'] for repeat in plot_data[entry]]

            if calculate_means is False:
                raw_data_positions.append(sum(x_list) / len(x_list))
                raw_data.append(y_list)
            else:
                x_mean = sum(x_list) / len(x_list)
                y_mean = sum(y_list) / len(y_list)

                x_error = self.calc_error_bars(x_list)
                y_error = self.calc_error_bars(y_list)

                plot_data[entry] = {'x_mean': x_mean, 'x_error': x_error, 'y_mean': y_mean, 'y_error': y_error}

        if calculate_means is True:
            return plot_data
        else:
            return raw_data, raw_data_positions

    def calc_error_bars(self, data_list):
        if self.error_bars == '1 SD':
            return np.std(data_list)
        elif self.error_bars == '2 SD':
            return 2 * np.std(data_list)
        else:
            return 0

    @staticmethod
    def check_constraint(data, data_constraints):

        for c in data_constraints:
            if c.constraint == 'none':  # This occurs if the user has chosen to not split the data
                continue

            operator = c.operator
            actual_val = data[c.constraint]
            required_val = c.value

            # Compare the values as floats if they are numbers, else compare as strings
            try:
                required_val = float(required_val)
            except ValueError:
                pass

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

    def basic_plots(self, split_data, plot_type, extra_constraints, trim_data, trim_data_pc, plot_style, error_bars):
        # Update the default style based on the input option
        if plot_style == 'line':
            self.default_style = GraphStyle.LINE
        elif plot_style == 'basic scatter':
            self.default_style = GraphStyle.SCATTER_BASIC
        elif plot_style == 'grouped scatter':
            self.default_style = GraphStyle.SCATTER_GROUPED
        elif plot_style == 'box plot':
            self.default_style = GraphStyle.BOX_PLOT

        self.trim_data = trim_data
        self.trim_data_percent = trim_data_pc
        self.error_bars = error_bars

        split_data_options = set()
        if split_data != 'none':
            for data in self.data_set:
                split_data_options.add(data[split_data])
        else:
            split_data_options.add(None)

        self.bar_plot_count = 0
        self.bar_plots_total = len(split_data_options)

        for option in split_data_options:
            constraints = [Constraint(split_data, '=', option)] + extra_constraints

            if plot_type == 'TRs vs q':
                plt.subplot(1, 1, 1)
                self.bar_plot_count += 1
                self.plot_data(plot_params['ftrs vs q'], constraints)
                self.plot_data(plot_params['rtrs vs q'], constraints)

            elif plot_type != 'all':
                plt.subplot(1, 1, 1)
                self.plot_data(plot_params[plot_type], constraints)

            else:
                plt.subplot(3, 3, 1)
                self.plot_data(plot_params['speed vs q'], constraints)

                plt.subplot(3, 3, 2)
                self.plot_data(plot_params['discovery vs q'], constraints)

                plt.subplot(3, 3, 3)
                self.plot_data(plot_params['decision vs q'], constraints)

                plt.subplot(3, 3, 4)
                self.plot_data(plot_params['implementation vs q'], constraints)

                plt.subplot(3, 3, 5)
                self.plot_data(plot_params['cohesion vs q'], constraints)

                plt.subplot(3, 3, 6)
                self.bar_plot_count += 1
                self.plot_data(plot_params['ftrs vs q'], constraints)
                self.plot_data(plot_params['rtrs vs q'], constraints)

                plt.subplot(3, 3, 7)
                self.plot_data(plot_params['speed vs cohesion'], constraints)

        plt.show()

    def t_tests(self, data_type, constraints, trim_data, trim_data_percent):
        self.trim_data = trim_data
        self.trim_data_percent = trim_data_percent

        if data_type == 'speed':
            pp = plot_params['speed vs q']
        else:  # data_type == cohesion
            pp = plot_params['cohesion vs q']

        raw_data, quorums = self.sort_plot_data(pp, constraints, calculate_means=False)
        # Truncate the raw data to the size of the quorum with the least repeats (if some quorums have more repeats than others)
        min_length = 100000
        for data in raw_data:
            if len(data) < min_length:
                min_length = len(data)
        for i in range(0, len(raw_data)):
            if len(raw_data[i]) > min_length:
                raw_data[i] = raw_data[i][0:min_length]

        p_values = []
        for quorum1 in raw_data:
            new_p_values = []
            p_values.append(new_p_values)
            for quorum2 in raw_data:
                if quorum1 == quorum2:
                    new_p_values.append('NaN')
                    continue

                new_p_values.append(stats.ttest_rel(quorum1, quorum2)[1])

        return p_values, quorums


class GraphStyle(Enum):
    LINE, BAR, SCATTER_BASIC, SCATTER_GROUPED, BOX_PLOT, DEFAULT = range(0, 6)


class PlotParameters:
    def __init__(self, graph_style, x_type, y_type, group_type, labels):
        self.graph_style = graph_style
        self.x_type = x_type
        self.y_type = y_type
        self.group_type = group_type
        self.labels = labels


class Constraint:
    def __init__(self, constraint, operator, value):
        self.constraint = constraint
        self.operator = operator
        self.value = value

plot_params = {
    'speed vs q': PlotParameters(GraphStyle.DEFAULT, 'quorum', 'end of emigration time', 'quorum', ['Quorum', 'Speed']),
    'discovery vs q': PlotParameters(GraphStyle.DEFAULT, 'quorum', 'discovery time', 'quorum', ['Quorum', 'Discovery Time']),
    'decision vs q': PlotParameters(GraphStyle.DEFAULT, 'quorum', 'first carry time', 'quorum', ['Quorum', 'Decision Time']),
    'implementation vs q': PlotParameters(GraphStyle.DEFAULT, 'quorum', 'implementation time', 'quorum', ['Quorum', 'Implementation Time']),
    'cohesion vs q': PlotParameters(GraphStyle.DEFAULT, 'quorum', 'absolute cohesion', 'quorum', ['Quorum', 'Cohesion']),
    'accuracy vs q': PlotParameters(GraphStyle.DEFAULT, 'quorum', 'accuracy', 'quorum', ['Quorum', 'Cohesion']),
    'ftrs vs q': PlotParameters(GraphStyle.BAR, 'quorum', 'ftrs', 'quorum', ['Quorum', 'Number of Runs']),
    'rtrs vs q': PlotParameters(GraphStyle.BAR, 'quorum', 'rtrs', 'quorum', ['Quorum', 'Number of Runs']),
    'speed vs cohesion': PlotParameters(GraphStyle.SCATTER_GROUPED, 'end of emigration time', 'absolute cohesion', 'quorum', ['Speed', 'Cohesion'])
}
