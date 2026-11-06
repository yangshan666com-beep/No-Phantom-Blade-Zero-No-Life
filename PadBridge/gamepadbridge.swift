import Foundation
import GameController

var controller: GCController?
var lastLine = ""
var prevCross = 0
var prevTriangle = 0
var prevUp = 0
var prevDown = 0
var prevLeft = 0
var prevRight = 0

func emit(_ text: String)
{
    print(text)
    fflush(stdout)
}

func handle(_ c: GCController)
{
    controller = c
    emit("READY \(c.vendorName ?? "controller")")
}

func detach()
{
    controller = nil
    emit("DISCONNECTED")
}

func pressEvent(_ name: String, _ value: inout Int, _ newValue: Int)
{
    if value == 0 && newValue == 1
    {
        emit("P \(name)")
    }
    value = newValue
}

NotificationCenter.default.addObserver(forName: .GCControllerDidConnect, object: nil, queue: nil)
{
    note in
    if let c = note.object as? GCController
    {
        handle(c)
    }
}

NotificationCenter.default.addObserver(forName: .GCControllerDidDisconnect, object: nil, queue: nil)
{
    _ in
    detach()
}

for c in GCController.controllers()
{
    handle(c)
}

FileHandle.standardInput.readabilityHandler =
{
    handle in
    if handle.availableData.isEmpty
    {
        exit(0)
    }
}

func poll()
{
    guard let c = controller, let p = c.extendedGamepad else
    {
        return
    }

    let lx = p.leftThumbstick.xAxis.value
    let ly = p.leftThumbstick.yAxis.value
    let l2 = p.leftTrigger.value
    let rx = p.rightThumbstick.xAxis.value
    let ry = p.rightThumbstick.yAxis.value
    let cross = p.buttonA.isPressed ? 1 : 0
    let triangle = p.buttonY.isPressed ? 1 : 0
    let up = p.dpad.up.isPressed ? 1 : 0
    let down = p.dpad.down.isPressed ? 1 : 0
    let left = p.dpad.left.isPressed ? 1 : 0
    let right = p.dpad.right.isPressed ? 1 : 0

    pressEvent("cross", &prevCross, cross)
    pressEvent("triangle", &prevTriangle, triangle)
    pressEvent("up", &prevUp, up)
    pressEvent("down", &prevDown, down)
    pressEvent("left", &prevLeft, left)
    pressEvent("right", &prevRight, right)

    let line = String(format: "PAD %.3f %.3f %.3f %.3f %.3f %d %d %d %d %d %d", lx, ly, l2, rx, ry, cross, triangle, up, down, left, right)
    if line != lastLine
    {
        lastLine = line
        emit(line)
    }
}

let timer = Timer(timeInterval: 1.0 / 60.0, repeats: true) { _ in poll() }
RunLoop.main.add(timer, forMode: .common)
RunLoop.main.run()
