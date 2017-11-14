
##Overview##
This documentation assumes use of the git command line. By nature of git commands the necessary actions in any GUI will be clear.

If you would like to use the command line, Git Bash is generally going to be a better experience than Windows Command Prompt. For example, Git Bash includes branch descriptors, tab completion etc. [Aliases](https://githowto.com/aliases) can be extremely helpful.

If you would like to use a GUI, [Git Extensions](http://git-extensions-documentation.readthedocs.io/en/latest/index.html#) is a robust option. A GUI will be infinitely more efficient for more in depth task including merge conflict resolution and cherry picking.

The general flow follows as:
`git add`
`git commit`
`git pull`
`git push`

##Broadcast Branch Structure##
### Master Branch ###

**Never commit anything to the master branch!**

### Hotfix Branch ###

Follow same instructions as release branch in context of the hotfix branch. Be sure to back merge into release and then make sure to back merge release into develop.

### Release Branch ###

**Bug Fixes**
*There is no need to create branches for release bug fixes. Instead work directly on the release branch.*

 1. `git checkout release` (origin/release)
 2. `git pull` (make sure you have the latest before making changes)
 3. make source changes
 4. `git status` (review and verify non-staged changes)
 5. `git add -A` (stage your changes, -A will add everything, including deleted files)
 6. `git commit -m"descriptive comment"` (commit changes locally)
 7. `git pull` (pull from remote release branch)
	 1. The following steps are only necessary if you have merge conflicts
	 2. Resolve any merge conflicts
	 3. `git add -A` (stage merge conflict resolution, -A will add everything, including deleted files)
	 4. `git commit -m"descriptive comment"` (commit changes locally)
	 5. `git push` (push commit(s) to remote branch release)

**Back Merge Release into Develop: Cherry Picking**

For now, the GUI is your best bet here. Cherry pick the committed sha hash from release into develop one-by-one for better merge conflict context.

If you'd rather take the time to do this by the command line:
 1. `git checkout develop` (checkout develop branch; this is the branch you'll merge your cherry-pick)
 2. `git cherry-pick a1b23c` (replace a1b23c with the SHA of the commit from release branch you want to cherry-pick into develop)
 3.  resolve any conflicts in preference of incoming cherry-pick from release
 4.  Either `git commit -m"descriptive comment"` to explicitly describe the cherry-pick commit, or `git commit` to be prompted with a auto-drafted commit message with the commit message from cherry-pick.
 5. `git push` (push to remote branch develop)

### Develop Branch ###
**Feature Branch Initialization**
*This only needs to be initialized by one developer. This developer must set the upstream origin in order for other team members to access it remotely via the server.*

*You only need to create feature branches for stories, all sub-tasks should be completed in this story branch. Do not create branches for sub-tasks.*

Feature branch nomenclature: **feature/BCOP123**
- Always include **feature/** preceding the story ticket identifier, this will organize all branches into a feature folder
- Do not include the dash in the story ticket identifier

 1. `git checkout develop`
 2. `git pull` (make sure you have the latest)
 3. `git branch feature/BCOP123` (create the branch from develop)
 4. `git checkout feature/BCOP123` (checkout the new branch)
 5. `git push --set-upstream origin feature/BCOP123` (set the origin to remote)
 
**Feature Branch Development**

 1. `git checkout feature/BCOP123`
 2. `git pull` (makes sure you have the latest before making changes)
 3. make source changes
 4. `git status` (review and verify non-staged changes)
 5. `git add -A` (stage your changes, -A will add everything, including deleted files) *.gitignore files that you never want to commit*
 6. `git commit -m"descriptive comment"` (commit changes locally)
 7. `git pull` (pull from remote branch feature/BCOP123)
	 1. Resolve any merge conflicts
	 2. `git add -A` (stage merge conflict resolution, -A will add everything, including deleted files)
	 3. `git commit -m"descriptive comment"` (commit changes locally)
 8. `git push` (push commit(s) to remote branch feature/BCOP123)
 9. `git pull origin develop` (pull down origin/develop into branch feature/BCOP123)
	 1. The following steps are only necessary if you have merge conflicts resulting from pulling develop into the feature branch
	 2. Resolve any merge conflicts
	 3. `git status` (review and verify changes)
	 4. `git add -A` (stage merge conflict resolution, -A will add everything, including deleted files)
	 5. `git commit -m"descriptive comment"` (commit changes locally)
 10. `git push` (push changes to remote branch feature/BCOP123)

**Squashing and Merging Feature Branches into Develop**

Squash and merge will combine all the changes on the feature branch into a single commit, and then add this commit to the tip of the develop branch. Generally, you will only be squashing and merging a feature branch into develop when the story is code reviewed, completed and ready to be deployed for QA review. It is important that the latest develop has been pulled down into the feature branch and **all** merge conflicts are resolved prior to squash and merge. If the feature branch does not have the latest develop code base then a possible merge conflict will occur at merge of feature branch into develop.

 1. `git checkout develop`
 2. `git pull` (make sure you have the latest source from remote)
 3. `git merge --squash feature/BCOP123`
 4. If you run into merge conflicts -- you need to resolve them. (This should not happen if you have successfully pulled develop into the feature branch and resolved conflicts prior to merging the feature branch into develop. It is also to your benefit to pull develop into the feature branch afterwards if there is intent to continue using the feature branch.) 
 5. Either `git commit -m"descriptive comment"` to explicitly describe the commit, or `git commit` to be prompted with a auto-drafted commit message with all commit messages that were squashed (vim basics -- `i` to insert, ESC to exit insert, `:wq` to save and exit vim, `:q` to exit vim)
 6. `git push` (push changes to remote develop)

**Feature Branch Deletion**
*delete feature branches once their story has been completed*

 1. `git branch -D feature/BCOP123` (deletes branch locally)
 2. `git push origin :feature/BCOP123` (deletes branch remote)

##Overview cont.##

**Some additional commands that may be helpful:**
`git branch --all` get a list of your current branch structure (both local and remote)
`git diff branch1 branch2` look at the diffs between two branches (i.e. `git diff release develop`)
`git rebase develop` [rebase](https://jeffkreeftmeijer.com/git-rebase/) your checked out feature branch from develop to keep your tree structure as clean as possible (in short: temporarily rewind your feature branch, pull down the latest from develop branch into your feature branch, replay your commits on top of new base)

**Ignoring Files**
If you are ignoring a file, file type, or directory via .gitignore that has already been committed and pushed to the remote repository you need to remove cache the file and commit the deletions in order for it to be deleted and ignored locally and remotely. You should be committing the modified .gitignore also. You can remove cache a file by running `git rm -r --cache [dirfilepath]`.

On `git status` if you are seeing the file(s) or directories as modified, then they have not been removed.