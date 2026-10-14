import pandas as pd
import seaborn as sns
import matplotlib.pyplot as plt
import sys
import re
import os
from datetime import datetime

class_palette = {"A (Rhodopsin)": "#edb2b2", "B1 (Secretin)": "#f4d799", "C (Glutamate)": "#add9aa", "F (Frizzled)": "#aad8d9"}
state_palette = {"Active": "#add9aa", "Intermediate": "#f4d799", "Inactive": "#edb2b2"}
metrics = ["braycurtis", "canberra", "chebyshev", "cityblock", "correlation", "cosine", "euclidean", "minkowski", "seuclidean", "sqeuclidean"]
states = {"all": "All", "active": "Act", "intermediate": "Intmd", "inactive": "Inact"}

def output(infile, outfile, sheet, index, skiprows, skipcols, palette, catecol, mlist):
    if all([os.path.isfile(outfile.replace("{0}", m)) for m in mlist]):
        print("[info][" + str(datetime.now()) + "] skipped sheet " + sheet + " in " + infile)
        return

    df = pd.read_excel(infile, sheet_name=sheet, index_col=index, skiprows=skiprows)
    df.drop(columns=df.columns[skipcols], inplace=True)
    colors = df[catecol].map(palette)
    df.drop(columns=catecol, inplace=True)
    df.dropna(axis="index", how="all", inplace=True)
    df.dropna(axis="columns", how="all", inplace=True)

    if len(df.index) < 2 or len(df.columns) < 2:
        print("[warn][" + str(datetime.now()) + "] insufficient data in sheet " + sheet + " in " + infile)
        return

    # Draw the full plot
    for m in mlist:
        #with pd.option_context('display.max_rows', None, 'display.max_columns', None):  # more options can be specified also
        out = outfile.replace("{0}", m)
        if os.path.isfile(out):
            print("[info][" + str(datetime.now()) + "] skipped existing " + out)
            continue
        print("[info][" + str(datetime.now()) + "] writing " + out)
        try:
            sns.clustermap(df, metric=m, center=0, cmap="seismic", linewidths=.75, figsize=(15,15), row_colors=colors, col_colors=colors)
            plt.savefig(out, dpi=400)
            plt.close()
        except:
            print("[error][" + str(datetime.now()) + "] failed to plot " + out)

if __name__ == '__main__':
    if len(sys.argv) < 2:
        print("usage: python " + sys.argv[0] + " <excel_file> [all|<metric_name>]")
        print("supported metrics: '" + "', '".join(metrics) + "'")
        exit()

    filename = sys.argv[1]
    base, ext = os.path.splitext(filename)

    if ext != ".xlsx":
        print("ERROR: only .xlsx files are supported")
        exit()

    mlist = ["correlation"]
    if len(sys.argv) > 2:
        if sys.argv[2] == "all":
            mlist = metrics
        elif sys.argv[2] in metrics:
            mlist = [sys.argv[2]]
        else:
            print("ERROR: only '" + "', '".join(metrics) + "' metrics are supported")
            exit()

    # Read the xlsx file
    if "all+" in base:
        for state, key in states.items():
            output(filename, base + '_' + state + "_{0}.png", key + " Sml", "Protein", [1,2,3], [1,2], class_palette, "Class", mlist)
    else:
        fields = re.split('[+_.]', re.split('[/\\\\]', filename)[-1])
        sheet_name = fields[0] + '_' + fields[-3] + " Similarities"
        output(filename, base + "_{0}.png", sheet_name, "PDB Id", [1], [], state_palette, "State", mlist)
