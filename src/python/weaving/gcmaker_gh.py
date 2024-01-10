import sys
from gcmaker_helper import generate_grasshopper_data_gh
from optimization_diagram import optimize_model_gh
from io_redirection import suppress_stdout as so
from webappio import generate_webapp_data
model_name = sys.argv[1]

if optimize_model_gh(model_name):
    print("Optimized: " + model_name)
    generate_grasshopper_data_gh(model_name)
