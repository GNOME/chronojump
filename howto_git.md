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

download from git repo

    git pull --rebase

Recuperate last stash

    git stash pop (without leaving any copy, recommended)
    git stash apply (storing a copy for the future)

Or can use autostash:
https://cscheng.info/2017/01/26/git-tip-autostash-with-git-pull-rebase.html


## Common use

### Change to a concrete hash:

    git checkout d1dba97d

Note that on Chronojump version is shown: 2.2.1-456 gd1dba97d2
g es for git. And the 2... or other chars is because hash is longer

You have some changes on a file (uncommited) but you don't wanted (and you don't want to stash, just delete the changes)

    git checkout -- <filename>


### Merge with a remote branch (but do not do it with my laptop, because has problems fetching from github)

    git remote add ylatuya https://github.com/ylatuya/chronojump
    git fetch ylatuya
    git merge ylatuya/macos-pkg
    or
    git merge ylatuya/mainline


### Problems with branchings

use gitg


### Do a patch with unstaged changes (like a backup)

    git diff > chronojump.patch
https://stackoverflow.com/a/15438863


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
