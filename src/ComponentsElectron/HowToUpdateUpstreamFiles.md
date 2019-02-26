## Updating the upstream/aspnetcore/browser.js directory

The contents of this directory come from https://github.com/aspnet/AspNetCore repo. I didn't want to use a real git submodule because that's such a giant repo, and I only need a few files from it here. So instead I used the `git read-tree` technique described at https://stackoverflow.com/a/30386041

One-time setup per working copy:

    git remote add -t release/3.0-preview3 --no-tags aspnetcore https://github.com/aspnet/AspNetCore.git

Then, to update the contents of upstream/aspnetcore/browser.js to the latest:

    git rm -rf upstream/aspnetcore
    git fetch --depth 1 aspnetcore
    git read-tree --prefix=src/ComponentsElectron/upstream/aspnetcore/browser.js -u aspnetcore/release/3.0-preview3:src/Components/Browser.JS/src
    git commit -m "Get browser.js files from commit a294d64a45f"

When using these commands, replace:

 * `release/3.0-preview3` with the branch you want to fetch from
 * `a294d64a45f` with the SHA of the commit you're fetching from

Longer term, we may consider publishing Components.Browser.JS as a NuGet package
with embedded .ts sources, so that it's possible to use inside a WebPack build
without needing to clone its sources.
