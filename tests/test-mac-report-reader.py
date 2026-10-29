"""Exercise the production Terminal reader under a POSIX shell; no macOS GUI required."""
import argparse, os, queue, subprocess, threading, time, uuid
from pathlib import Path
p=argparse.ArgumentParser()
p.add_argument('--tests-exe',required=True,type=Path)
p.add_argument('--shell',required=True,type=Path)
p.add_argument('--mono',type=Path)
a=p.parse_args()
root=Path(__file__).resolve().parents[1]
folder=root/'build/mac-report'/('reader-'+uuid.uuid4().hex)
folder.mkdir(parents=True)
log=folder/"report 'quoted' $HOME.log";stop=folder/"stop 'quoted' $HOME"
log.write_text('Header\n0\t1.25\n',encoding='utf-8')
def posix(path):
    if os.name!='nt':return str(path)
    return subprocess.check_output([str(a.shell),'--noprofile','--norc','-c','cygpath -u "$1"','_',str(path)],text=True).strip()
command=([str(a.mono)] if a.mono else [])+[str(a.tests_exe.resolve()),'--reader-script',posix(log),posix(stop)]
script=folder/'reader.command'
script.write_text(subprocess.check_output(command,text=True),encoding='utf-8',newline='\n')
subprocess.run([str(a.shell),'-n',str(script)],check=True)
proc=subprocess.Popen([str(a.shell),str(script)],stdout=subprocess.PIPE,stderr=subprocess.PIPE,text=True)
lines=queue.Queue();seen=[]
def read():
    for line in proc.stdout:lines.put(line.rstrip('\n'))
threading.Thread(target=read,daemon=True).start()
def until(expected):
    deadline=time.monotonic()+6
    while time.monotonic()<deadline:
        try:line=lines.get(timeout=0.1)
        except queue.Empty:continue
        seen.append(line)
        if line==expected:return
    raise AssertionError('Missing live row: '+expected+'; received '+repr(seen))
try:
    until('0\t1.25')
    with log.open('a',encoding='utf-8') as f:f.write('1\t0.5\nReport stream closed.\n')
    stop.write_text('',encoding='utf-8')
    until('Report stream closed.')
    assert proc.wait(timeout=5)==0
    assert seen==['Header','0\t1.25','1\t0.5','Report stream closed.'],seen
    assert proc.stderr.read()=='', 'Unexpected reader diagnostics'
    print('PASS production POSIX reader streams rows and final footer, stops cleanly, and quotes spaces/apostrophes/dollar signs')
finally:
    stop.write_text('',encoding='utf-8')
    if proc.poll() is None:
        try:proc.wait(timeout=5)
        except subprocess.TimeoutExpired:proc.kill()
