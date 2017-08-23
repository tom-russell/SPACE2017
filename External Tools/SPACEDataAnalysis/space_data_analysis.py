import tkinter as tk
from tkinter import filedialog
from tkinter import messagebox
import os
import numpy as np
import matplotlib
from matplotlib.figure import Figure
from matplotlib.backends.backend_tkagg import FigureCanvasTkAgg, NavigationToolbar2TkAgg

matplotlib.use("TkAgg")


class MainApp:
    def __init__(self, master):

        self.master = master

        options_frame = tk.Frame(master)
        options_frame.pack(side="left")
        self.button = tk.Button(options_frame, text="Load Emigration Data", command=self.load_data)
        self.button.pack(side="left")

        self.plot_frame = tk.Frame(master, height=600, width=800)
        self.plot_frame.pack(side="left")

        self.plot_label = tk.Label(master, text="Graph will appear here after data is loaded.")
        self.plot_label.pack(side="top")

    def load_data(self):
        self.plot_data([1, 2, 3, 4], [1, 2, 3, 4])
        pass

    def collect_emigration_data(self):
        # Allow the user to enter
        directory_path = filedialog.askdirectory(initialdir=os.getcwd(), mustexist=True,
                                                 title="Please select the data directory...")

        # Get a list of all the simulation result directories
        dirs = [d for d in os.listdir(directory_path) if os.path.isdir(os.path.join(directory_path, d))]

        # If an empty directory has been given
        if (len(dirs) == 0):
            messagebox.showerror("Data Error", "The given results directory is empty.")
            return

        for sub_dir in dirs:
            # gathering emigration accuracy data
            data_file_path = os.path.join(directory_path, sub_dir, "endSimData.txt")
            experiment_string = sub_dir.split('_')

            if os.path.getsize(data_file_path) == 0:
                continue

            file = open(data_file_path, 'r')
            nestAllegiances = file.readline().split(',')

            quorum = int(experiment_string[1][1:])
            if quorum not in presentQuorums:
                presentQuorums.append(quorum)

            repeat = int(experiment_string[2][1:])
            if repeat is 0:
                accuracyData[quorum] = []

            propBestNest = float(nestAllegiances[2]) / 200  # (float(nestAllegiances[1]) + float(nestAllegiances[2]))

            accuracyData[quorum].append(propBestNest)

            file.close()

            # gathering execution time data
            executionFilePath = os.path.join(batchPath, experimentDir, "execution.txt")
            file = open(executionFilePath, 'r')
            simulationTime = file.readlines()[3][21:].strip('\n').split(":")
            simTimeSecs = int(simulationTime[0]) * 3600 + int(simulationTime[1]) * 60 + int(simulationTime[2])

            if repeat is 0:
                timeData[quorum] = []

            timeData[quorum].append(simTimeSecs)
            file.close()

    def plot_data(self, x_data, y_data):
        f = Figure(figsize=(5, 5), dpi=100)
        a = f.add_subplot(1, 1, 1)
        a.plot(x_data, y_data)

        canvas = FigureCanvasTkAgg(f, self.plot_frame)
        canvas.show()
        canvas.get_tk_widget().pack(side=tk.TOP, fill=tk.BOTH, expand=True)

        toolbar = NavigationToolbar2TkAgg(canvas, self.plot_frame)
        toolbar.update()
        canvas.tkcanvas.pack(side=tk.TOP, fill=tk.BOTH, expand=True)


root = tk.Tk()
app = MainApp(root)

root.mainloop()
