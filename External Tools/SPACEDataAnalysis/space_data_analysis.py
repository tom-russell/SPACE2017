import tkinter as tk
from tkinter import filedialog
from tkinter import messagebox
import os
import matplotlib
from data_extractor import DataExtractor
from data_plotter import DataPlotter
from data_plotter import Constraint

matplotlib.use("TkAgg")


class MainApp:
    def __init__(self, master):

        self.master = master
        self.canvas = None

        menu_bar = tk.Menu(master)
        file_menu = tk.Menu(menu_bar, tearoff=0)
        file_menu.add_command(label="Load Emigration Data", command=self.load_data)
        file_menu.add_command(label="Save Last Plot")
        file_menu.add_separator()
        file_menu.add_command(label="Exit", command=master.quit)
        menu_bar.add_cascade(label="File", menu=file_menu)
        master.config(menu=menu_bar)

        control_f = tk.Frame(master, width=200, background='grey', relief=tk.RAISED, bd=2)
        control_f.pack_propagate(0)
        self.pack_controls_header(control_f, "Data Options:")

        data_options_f = tk.Frame(control_f)
        self.grid_row = 0

        self.split_on_var = tk.StringVar(value='none')
        self.split_options = self.label_and_field(data_options_f, 'Split Data On', 'OptionMenu', self.split_on_var, ('None',))

        self.best_nest_var = tk.IntVar()
        self.best_nest_var.set(2)
        self.label_and_field(data_options_f, "Best Nest ID", 'Entry', self.best_nest_var)

        self.max_sim_time_var = tk.IntVar()
        self.max_sim_time_var.set(120)
        self.label_and_field(data_options_f, "Max Sim Duration", 'Entry', self.max_sim_time_var)

        data_options_f.pack(ipady=3)

        self.pack_controls_header(control_f, "Display Plots:")
        tk.Button(control_f, text="Display All Basic Plots", command=lambda: self.display_plot('all')).pack(fill='x')
        tk.Button(control_f, text="Display Custom Plot").pack(fill='x')
        self.pack_controls_header(control_f, "Individual Plots:")
        self.plot_option = tk.StringVar(value=next(iter(plot_options.keys())))
        tk.OptionMenu(control_f, self.plot_option, *plot_options).pack(fill='x')
        display_individual_plot = lambda: self.display_plot(plot_options[self.plot_option.get()])
        tk.Button(control_f, text="Show Plot", command=display_individual_plot).pack(fill='x')

        self.pack_controls_header(control_f, "Plot Options:")
        plot_options_f = tk.Frame(control_f)
        self.trim_data = tk.BooleanVar()
        self.label_and_field(plot_options_f, 'Trim min/max Data', 'Checkbox', self.trim_data)
        self.trim_data_percent = tk.DoubleVar(value=5.0)
        self.label_and_field(plot_options_f, 'Trim Data %', 'Entry', self.trim_data_percent)

        plot_styles = ['line', 'basic scatter', 'grouped scatter', 'box plot']
        self.plot_style = tk.StringVar(value=plot_styles[0])
        self.label_and_field(plot_options_f, "Plot As", 'OptionMenu', self.plot_style, plot_styles)

        error_bar_opts = ['1 SD', '2 SD', 'none']
        self.error_bars = tk.StringVar(value=error_bar_opts[0])
        self.label_and_field(plot_options_f, 'Error Bars', 'OptionMenu', self.error_bars, error_bar_opts)

        plot_options_f.pack(fill='x')

        self.constraint_list = []
        constraint_buttons = tk.Frame(control_f)
        self.add_button = tk.Button(constraint_buttons, text="Add New Constraint", command=self.add_constraint, state=tk.DISABLED)
        self.add_button.pack(expand=1, fill='x', side='left')
        tk.Button(constraint_buttons, text="Remove", command=self.remove_constraint).pack(fill='x', side='left')
        constraint_buttons.pack(fill='x')
        constraints_frame = tk.Frame(control_f)
        scrollbar = tk.Scrollbar(constraints_frame, orient=tk.VERTICAL)
        self.constraints_box = tk.Listbox(constraints_frame, yscrollcommand=scrollbar.set, height=3)
        scrollbar.config(command=self.constraints_box.yview)
        scrollbar.pack(side=tk.RIGHT, fill=tk.Y)
        self.constraints_box.pack(fill='x')
        constraints_frame.pack(fill='x')

        self.pack_controls_header(control_f, "Pairwise T-Tests:")

        t_test_f = tk.Frame(control_f)
        self.two_tolerance = tk.BooleanVar()
        self.label_and_field(t_test_f, 'Show Both 0.05 & 0.01', 'CheckBox', self.two_tolerance)
        t_test_f.pack(fill='x')
        tk.Button(control_f, text='Speed-Quorum Tests', command=lambda: self.calculate_t_tests('speed')).pack(fill='x')
        tk.Button(control_f, text='Cohesion-Quorum Tests', command=lambda: self.calculate_t_tests('cohesion')).pack(fill='x')

        control_f.pack(side='left', padx=(10, 0), pady=10, ipadx=5, fill='y')

        self.data_frame = tk.Frame(master, width=700, bg='green')
        scrollbar_y = tk.Scrollbar(self.data_frame)
        scrollbar_y.pack(side=tk.RIGHT, fill=tk.Y)
        scrollbar_x = tk.Scrollbar(self.data_frame, orient=tk.HORIZONTAL)
        scrollbar_x.pack(side=tk.BOTTOM, fill=tk.X)
        self.list_box = tk.Listbox(self.data_frame, background='grey', yscrollcommand=scrollbar_y.set, xscrollcommand=scrollbar_x.set)
        self.list_box.insert('end', "No data is loaded, load via the File menu.")
        self.list_box.pack(fill='both', expand=True)
        scrollbar_y.config(command=self.list_box.yview)
        scrollbar_x.config(command=self.list_box.xview)

        self.data_frame.pack(side='left', fill='both', padx=10, pady=10, expand=True)

        self.data_set = None
        self.data_plot = None

    @staticmethod
    def pack_controls_header(parent, text):
        tk.Label(parent, text=text, fg='white', bg='#003489', relief=tk.RAISED, font=("", 12, "bold")).pack(fill='x', pady=(5, 0), ipady=5)

    def label_and_field(self, parent, text, field_type, answer_var, options=None):
        row = self.grid_row
        self.grid_row += 1
        parent.grid_columnconfigure(1, weight=1)

        tk.Label(parent, text=text).grid(row=row, column=0)

        if field_type == 'Entry':
            input_widget = tk.Entry(parent, textvariable=answer_var)
        elif field_type == 'OptionMenu':
            input_widget = tk.OptionMenu(parent, answer_var, *options)
        else:  # field_type == 'CheckButton':
            input_widget = tk.Checkbutton(parent, text="", variable=answer_var)

        input_widget.grid(row=row, column=1, sticky='W E')
        return input_widget

    def load_data(self):
        directory_path = filedialog.askdirectory(initialdir=os.getcwd(), mustexist=True,
                                                 title="Please select the data directory...")

        extractor = DataExtractor(directory_path, self.best_nest_var.get(), self.max_sim_time_var.get())
        invalid_files, unfinished_sims = extractor.extract_data()

        self.data_set = extractor.data_set
        self.data_plot = DataPlotter(self.data_set)
        msg_string = "%s simulations had missing or blank files.\n" % invalid_files
        msg_string += "%s simulations exceeded than the maximum time and so were removed." % unfinished_sims
        messagebox.showinfo('Data Loaded', msg_string)

        self.list_box.delete(0, tk.END)

        grid_row = 0
        for data in self.data_set:
            raw_data_string = ""
            for key, value in data.items():
                raw_data_string += "%s=%s, " % (key, value)
            grid_row += 1
            self.list_box.insert(tk.END, raw_data_string[:-2])
            if grid_row % 2 == 0:
                self.list_box.itemconfig(tk.END, bg='#e0e0e0')
            else:
                self.list_box.itemconfig(tk.END, bg='#f4f4f4')

        # Updating the list of options to split the data by
        options = self.data_set[0].keys()
        menu = self.split_options["menu"]
        menu.delete(0, "end")
        menu.add_command(label='none', command=lambda: self.split_on_var.set('none'))
        for string in options:
            menu.add_command(label=string, command=lambda option=string: self.split_on_var.set(option))

        self.add_button.config(state=tk.ACTIVE)

    def display_plot(self, plot_type):
        if self.data_plot is None:
            messagebox.showwarning('No Data Set Loaded', 'A data set must be loaded before plots can be displayed.')

        split_on = self.split_on_var.get()
        trim = self.trim_data.get()
        trim_pc = self.trim_data_percent.get()
        style = self.plot_style.get()
        err_bars = self.error_bars.get()
        self.data_plot.basic_plots(split_on, plot_type, self.constraint_list, trim, trim_pc, style, err_bars)

    def display_custom_plot(self):
        pass

    def add_constraint(self):
        pop_window = tk.Toplevel(self.master)
        add_constraint_f = tk.Frame(pop_window)
        keys = self.data_set[0].keys()
        constraint = tk.StringVar(value=next(iter(keys)))
        tk.OptionMenu(add_constraint_f, constraint, *keys).grid(row=0, column=0)
        operator = tk.StringVar(value='=')
        tk.OptionMenu(add_constraint_f, operator, '=', '!=', '<', '>', '<=', '>=').grid(row=0, column=1)
        value = tk.StringVar()
        tk.Entry(add_constraint_f, textvariable=value).grid(row=0, column=2)
        confirm_add = lambda: self.confirm_add_constraint(constraint.get(), operator.get(), value.get(), pop_window)
        tk.Button(add_constraint_f, text="Add", command=confirm_add).grid(row=1, column=0, columnspan=2)
        tk.Button(add_constraint_f, text="Cancel", command=lambda: self.close_window(pop_window)).grid(row=1, column=2)

        add_constraint_f.pack()

    def confirm_add_constraint(self, constraint, operator, value, window):
        self.constraint_list.append(Constraint(constraint, operator, value))
        self.constraints_box.insert(tk.END, "%s %s %s" % (constraint, operator, value))
        window.destroy()

    def remove_constraint(self):
        if len(self.constraints_box.curselection()) < 1:
            return

        index = self.constraints_box.curselection()[0]
        del(self.constraint_list[index])
        self.constraints_box.delete(index)

    @staticmethod
    def close_window(window):
        window.destroy()

    def calculate_t_tests(self, data_type):
        p_values, quorums = self.data_plot.t_tests(data_type, self.constraint_list, self.trim_data.get(), self.trim_data_percent.get())

        pop_window = tk.Toplevel(self.master, bg='black')

        for i in range(0, len(quorums)):
            tk.Label(pop_window, text=int(quorums[i]), bg='grey').grid(row=i+1, column=0, sticky='N S E W', pady=1, padx=1)
            tk.Label(pop_window, text=int(quorums[i]), bg='grey').grid(row=0, column=i+1, sticky='N S E W', pady=1, padx=1)

        upper_threshold = 0.10 # 0.05
        lower_threshold = 0.05 # 0.01
        for x in range(0, len(quorums)):
            for y in range(0, len(quorums)):
                if p_values[x][y] == 'NaN':
                    value = 'NaN'
                    colour = 'grey'
                else:
                    value = round(p_values[x][y], 3)
                    if self.two_tolerance.get() == True and value > lower_threshold and value <= upper_threshold:
                        colour = 'orange'
                    elif value > upper_threshold:
                        colour = 'red'
                    else:
                        colour = 'green'

                tk.Label(pop_window, text=value, bg=colour).grid(row=x+1, column=y+1, sticky='N S E W', pady=1, padx=1)
                tk.Grid.rowconfigure(pop_window, x+1, weight=1)
                tk.Grid.columnconfigure(pop_window, y+1, weight=1)


plot_options = {
    "Speed vs Quorum": "speed vs q",
    "Discovery Time vs Quorum": "discovery vs q",
    "Decision Time vs Quorum": "decision vs q",
    "Implementation Time vs Quorum": "implementation vs q",
    "Cohesion vs Quorum": "cohesion vs q",
    "Accuracy vs Quorum": "accuracy vs q",
    "FTR/RTRs vs Quorum": "TRs vs q",
    "Cohesion vs Speed": "speed vs cohesion"
}

root = tk.Tk()
root.title('SPACE Data Analysis Tool')
root.geometry('900x700')
app = MainApp(root)

root.mainloop()
