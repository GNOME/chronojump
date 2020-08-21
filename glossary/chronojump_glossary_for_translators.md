# Glossary for Chronojump Translators

### March, 27, 2007

ChronoJump, is a "complete multiplatform system for measurement,
management and statistics of the jump, run, reaction time and pulse
tests" <http://www.gnome.org/projects/chronojump/>  
The knowledge of different the kinds of jump, the specific terminology
and the concepts surrounding other "tests" which are measured, like
runs, reaction times and pulses, may difficult the translation of the
software. The purpose of this document is to bring a clear context to
the translator and offer a small glossary of the terms that can be
confusing. There is also an intentional need to write this document
briefly, this guide was thought for being short and
practical.

![](http://www.gnome.org/projects/chronojump/images/chronojump_esquema_jump_300.png)

### Context: An sports evaluation

Commonly, the sports instructor, physical educator, trainer,
kyneologist... apply some tests to one or more people who jump (referred
sometimes as "persons" or "jumpers" in jump tests) using an instrument
nowadays connected to a computer. Typically people are evaluated
altogether in a day. As an example, the physical educator can ask every
student jump in three different ways. This evaluation can developed at
the beginning and in the end of the course, and the good thing is that
the evaluator is able to compare the results of every student in the
same day, or compare the evolution along two days. Every day of
evaluation is called a "session". Each session has a name, place, date,
possible comments, and also people who perform a specific jump, run or
pulses.

### Glossary:

##### Abalakov Jump

(also called "ABK") a kind of [simple jump](#jump) (we recommend not
to translate the term).

##### ABK

(See [Abalakov jump](#abalakovjump)).

##### Bell

Chronojump uses a software bell as an auditive and configurable way to
tell the [Jumper](#jumper) or [Runner](#runner) info about the parameters
of the [test](#test): [TC](#tc), [TF](#tf), TF/TC and time.

##### Chronojump

Sometimes referred as the free (GPL) software for managing the data
coming from the [Chronopic](#chronopic) [chronometer](#chronometer), and
sometimes referred as the whole project: Software, Hardware,
documentation, website, ...

##### Chronometer

Instrument used for measuring time. In Chronojump project we use
[Chronopic](#chronopic).

##### Chronopic

Open Hardware used for recording and managing the changes detected on
the [Platform](#platform). At this moment there are two Chronopic models
(both using same firmware):

- One is derived from Skypic: open hardware for controlling little
  robots, vastly used but with lots of unnecessary components.This link
  points out how to build it and how to buy it
  <http://www.gnome.org/projects/chronojump/construction.html>
  
- The other is made only with the necessary components. This link points
  out how to build it
  <http://www.gnome.org/projects/chronojump/construction.html>,
  currently there are no manufacturers selling this model.

![](http://www.gnome.org/projects/chronojump/images/chronopic_foto.jpg)

##### CMJ

(See [Countermovement jump](#countermovementjump)).

##### Countermovement Jump

(abbreviated "CMJ") a kind of [simple jump](#jump) (we recommend not to
translate the term).

##### Contact platform

(See [Platform](#platform)).

##### Contact time

Time where the person is in the platform. Also referred as
[Time of Contact](#timeofcontact) or [TC](#tc).

##### DJ

(See [Drop jump](#dropjump)).

##### Drop Jump

(abbreviated "DJ") a kind of [simple jump](#jump) where there's a
previous fall from a determined height and then jumps. On this jump,
the recorded data is one [contact time](#contacttime), and one
[flight time](#flightime). (we recommend not to translate the term).

##### Finish

Action of ending a test by pressing a button. This action can be done
for finishing an unlimited [test](#test), or making shorter a
[reactive jump](#jump), [intervallic run](#run) or [pulse](#pulse).

##### Fligh time

Time where the person is not in contact with the platform. Also referred
as [Time of Flight](#timeofflight) or [TF](#tf).

##### Jump

jump is one of the tree [tests](#test) that are currently evaluated.
Also there are two kind of jumps: *Simple* and *Reactive*.

 - **Simple jump**  
   jump that have only one [flight time](#flightime). On this jump,
   jumper starts and ends inside the [platform](#platform). There's a
   special type called [Drop Jump](#dropjump) where jumper falls
   down from a predefined height (called "falling height"), and then
   jumps. In this case, the recorded data is one
   [contact time](#contacttime), and one [flight time](#flightime).
   
 - **Reactive jump**  
   (abbreviated "RJ") this jump is described as a number of repetitive
   or reactive jumps performed consecutively. All the
   [contact times](#contacttime), and [flight times](#flightime) are
   recorded. It's common to try to maintain as much time as possible
   low contact and height flight time. This jump is limited (ended) by:
   
    - the number of jumps: "Perform 'n' jumps"
    
    - the jumping time: "Jump during 'n' seconds"
    
    - unlimited: "Jump, and trainer will tell you when to stop".
    
##### Jumper

The name of a [person](#person) is called when does a [jump](#jump).

##### Load person

As all data is stored in a database, there's no need to define or create
a new [person](#person) twice, if one person is evaluated in one
[session](#session), and we want to re-evaluate in another session,
we call this action "Load Person", and it has to be understood as
"To use this known person also in current session".

##### Person

The human who executes the tests. Depending on what he is doing, may be
called as [Jumper](#jumper) or [Runner](#runner). Person does not
receive a special name when is performing [reaction time](#reactiontime)
or [pulse](#pulse) tests.

##### Platform

It's an instrument used for knowing if a person is on the floor or in
the air. A person can have two states on the platform: inside or outside.
On [jumps](#jump), the time that a person is in the air
([flight time](#flightime)) is used to find the height of the Center of
Gravity of the jumper. On [Runs](#run) the time that the person is not in
the platform is used to determine the speed. The platform can be easily
built following this instructions:  
<http://www.gnome.org/projects/chronojump/construction_contact_platform.html>

![](http://www.gnome.org/projects/chronojump/images/plataforma_contactos_final.jpg)

##### Pulse

We call 'pulse' to the ability of a person on following a rhythm
(predefined or natural), and when this rhythm is primary: only one
repeating beat. The pulse contains a succession of equal beats limited
by time or unlimited (instructor decides when to stop). On pulses we
want to know the number of beats per second and the rhythm of the pulse
as time passes.

![](http://www.gnome.org/projects/chronojump/images/chronojump_esquema_pulse_300.png)

##### Reaction time

Time between a signal (chronopic light) and a response (user clicks on
Chronopic or touches platform).

![](http://www.gnome.org/projects/chronojump/images/chronojump_esquema_reaction_time_300.png)

##### Report

A detailed document containing the data of the [session](#session)
(date, place, comments), the [persons](#person) who participated,
the [tests](#test) developed, and [statistics](#statistic) evaluated.

##### Run

Run is one of the tree [tests](#test) that are currently evaluated.
Also there are two kind of runs: *Simple* and *Intervallic*.

 - **Simple run:**  
   run that have only one [flight time](#flightime). On this
   run, runner starts on the [platform](#platform), or a few
   meters before it. The run stop when the runner arrives to a
   second platform. It can be also done with only one platform in
   a circular track. The ony registered data is the time that the
   person is between the two platforms. This time is normally
   converted to an average speed, knowing the distance between
   both.
   
 - **Intervallic run:**  
   this run is described as a number of individual runs called
   [tracks](#track). Every time each track are recorded, and
   it's also converted to speed, as all the tracks have the same
   distance. This run is limited (ended) by:
   
    - the number of tracks: "Run 'n' tracks"
    
    - the running time: "Run during 'n' seconds"
    
    - unlimited: "Run, and the instructor will tell you when to
      stop".

![](http://www.gnome.org/projects/chronojump/images/chronojump_esquema_run_300.png)

##### Runner

The name of a [person](#person) is called when does a [run](#run).

##### RJ

(See [reactive jump](#jump)).

##### Session

(See [Context: An Sports Evaluation](#context-an-sports-evaluation)).

##### SJ

(See [Squat jump](#squatjump)).

##### Squat Jump

(abbreviated "SJ") a kind of [simple jump](#jump) (we recommend not to
translate the term).

##### Statistic

Evaluation of one or more tests presented on a tabular and/or graphical
way.

##### Test
On Chronojump we use this word for referring [Jump](#jump), [Run](#run),
[Reaction time](#reactiontime) or [Pulse](#pulse). All of them are
time-related tests.

##### TC

(See [contact time](#contacttime)).

##### TF

(See [flight time](#flightime)).

##### Time of contact

(See [contact time](#contacttime)).

##### Time of flight

(See [flight time](#flightime)).

##### Track

Is the distance between two platforms in a Intervallic [Run](#run).