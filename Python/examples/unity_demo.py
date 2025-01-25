import json

import threading, struct

from src.enums.event import Event
from src.enums.finger import Finger
from src.loop.cv_input import CVInput
from src.state.vectors.vector3 import Vector3
from src.neural.static_recogniser_from import StaticRecogniserFrom

import socket

with open("./config.json", "r") as file:
    config = json.load(file)

with open(config["labels"]) as file:
    labels = json.load(file)

local_socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
local_socket.bind(('localhost', 1800))
local_socket.listen(1)

print("awaiting connection from unity...")
local_connection, _ = local_socket.accept()

flag = False
def on_right_hand_detect(data):
    global flag
    flag = data

angles = [0, 0, 0]
def on_hand_update(data):
    angles[0] = data["yaw"]
    angles[1] = data["pitch"]
    angles[2] = data["roll"]

delta = Vector3.zero()
def on_index_update(data):
    finger, new = data["finger"], data["new"]

    if finger.closed():
        delta.update(finger.tip() - new[-1])
    else:
        delta.update(Vector3.zero())

scale = 0
label = None
def on_post(data):
    global label, scale

    if data["label"] != label and data["confidence"].numpy() > 0.85:
        label = data["label"]

    encoded = None
    if label is None:
        encoded = "".encode('utf-8')
    else:
        encoded = label.encode('utf-8')

    scale_diff = 0
    if label == "scale":
        dist = Vector3.distance(cv_input.right().finger(Finger.Index).tip(), cv_input.right().finger(Finger.Thumb).tip())
        scale_diff = dist - scale
        scale = dist
    else:
        scale = 0

    if flag:
        data = struct.pack(f"<fffffffI{len(encoded)}s", angles[0], angles[1], angles[2], delta.x, delta.y, delta.z, scale_diff, len(encoded), encoded)
        local_connection.send(data)

cv_input = CVInput(config)
cv_input.with_model(StaticRecogniserFrom(config))

cv_input.right().with_process(Event.Detect, "base", on_right_hand_detect)
cv_input.right().with_process(Event.Update, "base", on_hand_update)

cv_input.right().finger(Finger.Index).with_process(Event.Update, "on_index_update", on_index_update)

cv_input.with_process(Event.Post, "on_change", on_post)

thread = threading.Thread(target = cv_input.start)

try:
    thread.start()
    input()
finally:
    cv_input.end()
    thread.join()

    local_connection.close()
    local_socket.close()