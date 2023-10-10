# Howto git

check how is your code right now:

    git diff
    git log
    git show

(or use gitg if there's something messy with branches)

## A) There's stuff and you want to commit it

    git commit -a

commit pieces of files

    git commit -p

commit pieces of a files

    git commit <filename> -p

push to repo

    git push

download from git repo (if there are problems, check *Merge problems* below)

    git pull --rebase

## B) There's stuff and you don't want to commit it now

Check if there are stashed things

    git stash list

Perform an stash of your current code

    git stash
    git stash push -m "stashedWithSomeName"

More stash list options https://stackoverflow.com/a/10726185

Show the files in the most recent stash:

    git stash show

Show the changes of the most recent stash:

    git stash show -p

Show the changes of the named stash:

    git stash show -p stash@{1}
    git stash show -p 1

Delete last stash (warning: no confirmation), or any stash, or all

    git stash drop
    git stash drop stash@{1}
    git stash clear

download from git repo

    git pull --rebase

Recuperate last stash

    git stash pop (without leaving any copy, recommended)
    git stash apply (storing a copy for the future)

Or can use autostash:
https://cscheng.info/2017/01/26/git-tip-autostash-with-git-pull-rebase.html

if there is a merge problem on git stash pop https://stackoverflow.com/a/27382210

    resolve conflicts (search ====)
    git restore --staged .
    git stash drop

## Common use

### Change commit message (unpushed)

    git commit --amend

### Undo a commit (unpushed)

    git reset --soft HEAD~1

Then you will not see differences with git diff, and git commit -p will not work as expected, but the written code exists (just commit is undone).

### Change to a concrete hash:

    git checkout d1dba97d

Note that on Chronojump version is shown: 2.2.1-456 gd1dba97d2
g es for git. And the 2... or other chars is because hash is longer

You have some changes on a file (uncommited) but you don't wanted (and you don't want to stash, just delete the changes)

    git checkout -- <filename>

### Use a concrete commit without all the previous

On Windows or Mac compilation machine, we createed a version. But a bug appeared, we fix, so we want to use that commit but not all the new commits pushed since last version

Update references (then git local will know about the commit)

    git fetch

Pick the commit (eg af8e0132)

    git cherry-pick af8e0132

If any problem:

    search with grep -r "===="
    fix thefile
    git add thefile
    git commit

If problem is big or better not apply that cherry:

    git cherry-pick --abort

If all ok:

    git push

### Cherry branches and new versions

- All commits are in master
- cherry2.3.0 is the first branch for fixes. There will be one for each version.
- when a new fix-bug version is needed, do:
    - git checkout cherry2.3.0
    - git cherry-pick (commit), fix merges, see above
    - on Windows and mac virtual machines, and linux computers in our lab be on cherry2.3.0 branch
    - compile, publish
- when a new version with new features is done, (eg 2.3.1) then:
    - from master create new branch git checkout -b cherry2.3.1
    - push it with: git push -u origin cherry2.3.1
    - on Windows and mac virtual machines, and linux computers on our lab: git checkout cherry2.3.1
    - compile, publish
- working like this we have the historical of any version and can go to the branch and cherry-pick more commmits if needed


### Merge with a remote branch (but do not do it with my laptop, because has problems fetching from github)

    git remote add ylatuya https://github.com/ylatuya/chronojump
    git fetch ylatuya
    git merge ylatuya/macos-pkg
    or
    git merge ylatuya/mainline


### Problems with branchings

use gitg


### Create a patch with unstaged changes (like a backup)

    git diff > chronojump.patch
https://stackoverflow.com/a/15438863

### Apply a patch

git apply -v chronojump.patch

### Mark current change as stable (or testing)

    git checkout stable

(for a new branch would be: git -b checkout stable)

    git rebase master

    git push

(for a new branch would be: git push --set-upstream origin stable)


### Use stable (or testing) on client

    git checkout stable


## Merge problems

There are conflicts to be solved.
Open the file/s affected
Find ======     Solve the problems (HEAD) is what there is on git repo

    git add <filename>
    git rebase -- continue
    git push

If you want to abort a merge done on a git stash pop (discarding the stashed stuff)

	git reset --merge

if the problem is on git stash pop see "merge problem on git stash pop" above
